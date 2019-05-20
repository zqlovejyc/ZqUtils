#region License
/***
 * Copyright © 2018-2019, 张强 (943620963@qq.com).
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * without warranties or conditions of any kind, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
/****************************
* [Author] 张强
* [Date] 2018-10-29
* [Describe] MongoDB工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// MongoDB工具类
    /// </summary>
    public class MongodbHelper
    {
        #region 私有字段
        /// <summary>
        /// 链接字符串
        /// </summary>
        private readonly string connectionString = ConfigHelper.GetAppSettings<string>("mongodbConnectionString") ?? "mongodb://localhost:27017";

        /// <summary>
        /// 数据库，若不存在，则自动创建
        /// </summary>
        private readonly string databaseName = ConfigHelper.GetAppSettings<string>("mongodbDatabase") ?? "Database";

        /// <summary>
        /// MongoClient
        /// </summary>
        private readonly MongoClient client;

        /// <summary>
        /// IMongoDatabase
        /// </summary>
        private readonly IMongoDatabase database;
        #endregion

        #region 公有属性
        /// <summary>
        /// 静态单例
        /// </summary>
        public static MongodbHelper Instance => SingletonHelper<MongodbHelper>.GetInstance();
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public MongodbHelper()
        {
            this.client = new MongoClient(this.connectionString);
            this.database = this.client.GetDatabase(this.databaseName);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="databaseName">数据库</param>
        public MongodbHelper(string databaseName)
        {
            this.databaseName = databaseName;
            this.client = new MongoClient(this.connectionString);
            this.database = this.client.GetDatabase(this.databaseName);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="databaseName">数据库</param>
        /// <param name="settings">MongoClientSettings配置</param>
        public MongodbHelper(string databaseName, MongoClientSettings settings)
        {
            this.databaseName = databaseName;
            this.client = new MongoClient(settings);
            this.database = this.client.GetDatabase(this.databaseName);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="databaseName">数据库</param>
        /// <param name="connectionString">链接字符串</param>
        /// <param name="isMongoClientSettings">是否为MongoClientSettings连接字符串，默认：false</param>
        public MongodbHelper(string databaseName, string connectionString, bool isMongoClientSettings = false)
        {
            this.databaseName = databaseName;
            this.connectionString = connectionString;
            if (isMongoClientSettings)
                this.client = new MongoClient(MongoClientSettings.FromConnectionString(connectionString));
            else
                this.client = new MongoClient(this.connectionString);
            this.database = this.client.GetDatabase(this.databaseName);
        }
        #endregion

        #region 构建Mongo的更新表达式
        /// <summary>
        /// 构建Mongo的更新表达式
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="fieldList">更新字段集合</param>
        /// <param name="property">属性</param>
        /// <param name="propertyValue">属性值</param>
        /// <param name="item">更新子项</param>
        /// <param name="father">父级字段</param>
        public static void BuildUpdateDefinition<T>(List<UpdateDefinition<T>> fieldList, PropertyInfo property, object propertyValue, T item, string father)
        {
            #region 复杂类型
            if (property.PropertyType.IsClass && property.PropertyType != typeof(string) && propertyValue != null)
            {
                #region 数组
                if (propertyValue.GetType().IsArray)
                {
                    var elementType = propertyValue.GetType().GetElementType();
                    if (propertyValue is IList arr && arr?.Count > 0 && arr[0].GetType() == elementType)
                    {
                        for (int index = 0; index < arr.Count; index++)
                        {
                            //复杂类型
                            if (elementType.IsClass && elementType != typeof(string))
                            {
                                foreach (var subInner in elementType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                                {
                                    if (string.IsNullOrWhiteSpace(father))
                                    {
                                        BuildUpdateDefinition(fieldList, subInner, subInner.GetValue(arr[index]), item, $"{property.Name}.$");
                                    }
                                    else
                                    {
                                        BuildUpdateDefinition(fieldList, subInner, subInner.GetValue(arr[index]), item, $"{father}.{property.Name}.$");
                                    }
                                }
                            }
                            //简单类型
                            else if (property.Name != "_id" && arr[index] != null)
                            {
                                if (string.IsNullOrWhiteSpace(father))
                                {
                                    fieldList.Add(Builders<T>.Update.Set($"{property.Name}.$", arr[index]));
                                }
                                else
                                {
                                    fieldList.Add(Builders<T>.Update.Set($"{father}.{property.Name}.$", arr[index]));
                                }
                            }
                        }
                    }
                }
                #endregion

                #region 集合
                else if (typeof(IList).IsAssignableFrom(propertyValue.GetType()))
                {
                    foreach (var sub in property.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        if (propertyValue is IList arr && arr?.Count > 0 && arr[0].GetType() == sub.PropertyType)
                        {
                            for (int index = 0; index < arr.Count; index++)
                            {
                                //复杂类型
                                if (sub.PropertyType.IsClass && sub.PropertyType != typeof(string))
                                {
                                    foreach (var subInner in sub.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                                    {
                                        if (string.IsNullOrWhiteSpace(father))
                                        {
                                            BuildUpdateDefinition(fieldList, subInner, subInner.GetValue(arr[index]), item, $"{property.Name}.$");
                                        }
                                        else
                                        {
                                            BuildUpdateDefinition(fieldList, subInner, subInner.GetValue(arr[index]), item, $"{father}.{property.Name}.$");
                                        }
                                    }
                                }
                                //简单类型
                                else if (property.Name != "_id" && arr[index] != null)
                                {
                                    if (string.IsNullOrWhiteSpace(father))
                                    {
                                        fieldList.Add(Builders<T>.Update.Set($"{property.Name}.$", arr[index]));
                                    }
                                    else
                                    {
                                        fieldList.Add(Builders<T>.Update.Set($"{father}.{property.Name}.$", arr[index]));
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                #region 实体
                else
                {
                    foreach (var sub in property.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        if (string.IsNullOrWhiteSpace(father))
                        {
                            BuildUpdateDefinition(fieldList, sub, sub.GetValue(propertyValue), item, property.Name);
                        }
                        else
                        {
                            BuildUpdateDefinition(fieldList, sub, sub.GetValue(propertyValue), item, $"{father}.{property.Name}");
                        }
                    }
                }
                #endregion
            }
            #endregion

            #region 简单类型
            else
            {
                //更新集中不能有实体键_id
                if (property.Name != "_id" && propertyValue != null)
                {
                    if (string.IsNullOrWhiteSpace(father))
                    {
                        fieldList.Add(Builders<T>.Update.Set(property.Name, propertyValue));
                    }
                    else
                    {
                        fieldList.Add(Builders<T>.Update.Set($"{father}.{property.Name}", propertyValue));
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// 构建Mongo的更新表达式
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entity">更新实体</param>
        /// <returns>返回更新属性集合</returns>
        public static List<UpdateDefinition<T>> BuildUpdateDefinition<T>(T entity)
        {
            var fieldList = new List<UpdateDefinition<T>>();
            foreach (var property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                BuildUpdateDefinition(fieldList, property, property.GetValue(entity), entity, string.Empty);
            }
            return fieldList;
        }
        #endregion

        #region 获取字段名称
        /// <summary>
        /// 获取字段名称
        /// </summary>        
        /// <param name="property">属性</param>
        /// <param name="entity">更新实体</param>
        /// <param name="father">父级字段</param>
        /// <param name="isExists">是否存在</param>
        /// <returns>返回字段名称</returns>
        public static string GetField(PropertyInfo property, object entity, string father, ref bool isExists)
        {
            foreach (var prop in property.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                //数组
                if (prop.PropertyType.IsArray)
                {
                    if (prop.PropertyType.GetElementType() == entity.GetType())
                    {
                        father += "." + prop.Name;
                        isExists = true;
                    }
                }
                //集合
                else if (typeof(IList).IsAssignableFrom(prop.PropertyType))
                {
                    if (prop.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Any(o => o.PropertyType == entity.GetType()))
                    {
                        father += "." + prop.Name;
                        isExists = true;
                    }
                }
                //对象
                else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    father += "." + GetField(prop, entity, prop.Name, ref isExists);
                }
            }
            return father;
        }

        /// <summary>
        /// 获取字段名称集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="entity">更新实体</param>
        /// <returns>返回符合条件的字段集合</returns>
        public static List<string> GetFields<T>(object entity)
        {
            var fields = new List<string>();
            foreach (var property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                //数组
                if (property.PropertyType.IsArray)
                {
                    if (property.PropertyType.GetElementType() == entity.GetType())
                    {
                        fields.Add(property.Name);
                    }
                }
                //集合
                else if (typeof(IList).IsAssignableFrom(property.PropertyType))
                {
                    if (property.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Any(o => o.PropertyType == entity.GetType()))
                    {
                        fields.Add(property.Name);
                    }
                }
                //对象
                else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    var isExists = false;
                    var field = GetField(property, entity, property.Name, ref isExists);
                    if (isExists)
                    {
                        fields.Add(field);
                    }
                }
            }
            return fields;
        }
        #endregion

        #region 索引
        #region 同步方法
        #region CreateIndex
        /// <summary>
        /// 创建索引
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="property">属性</param>
        /// <param name="collection">mongo集合</param>
        /// <param name="father">父级字段</param>
        public void CreateIndex<T>(PropertyInfo property, IMongoCollection<T> collection, string father)
        {
            foreach (var prop in property.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                //判断是否有索引
                var customAttributes = prop.GetCustomAttributes(typeof(MongoIndexAttribute), false);
                if (customAttributes?.Length > 0 && customAttributes.FirstOrDefault() is MongoIndexAttribute mongoIndex)
                {
                    var name = (string.IsNullOrWhiteSpace(father) ? prop.Name : $"{father}.{prop.Name}");
                    var keys = mongoIndex.Ascending ?
                            Builders<T>.IndexKeys.Ascending(name) :
                            Builders<T>.IndexKeys.Descending(name);
                    var model = new CreateIndexModel<T>(keys, new CreateIndexOptions
                    {
                        Name = mongoIndex.Name,
                        Unique = mongoIndex.Unique
                    });
                    collection.Indexes.CreateOne(model);
                }
                //实体
                else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    this.CreateIndex(prop, collection, (string.IsNullOrWhiteSpace(father) ? prop.Name : $"{father}.{prop.Name}"));
                }
            }
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        public void CreateIndex<T>(string collectionName = null)
        {
            var collection = this.database.GetCollection<T>(collectionName ?? typeof(T).Name);
            var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in props)
            {
                //判断是否有索引
                var customAttributes = prop.GetCustomAttributes(typeof(MongoIndexAttribute), false);
                if (customAttributes?.Length > 0 && customAttributes.FirstOrDefault() is MongoIndexAttribute mongoIndex)
                {
                    var keys = mongoIndex.Ascending ?
                            Builders<T>.IndexKeys.Ascending(prop.Name) :
                            Builders<T>.IndexKeys.Descending(prop.Name);
                    var model = new CreateIndexModel<T>(keys, new CreateIndexOptions
                    {
                        Name = mongoIndex.Name,
                        Unique = mongoIndex.Unique
                    });
                    collection.Indexes.CreateOne(model);
                }
                //实体
                else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    this.CreateIndex(prop, collection, prop.Name);
                }
            }
        }
        #endregion

        #region DropIndex
        /// <summary>
        /// 删除索引
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="indexName">索引名称</param>
        /// <param name="collectionName">表名</param>
        public void DropIndex<T>(string indexName, string collectionName = null)
        {
            var collection = this.database.GetCollection<T>(collectionName ?? typeof(T).Name);
            collection.Indexes.DropOne(indexName);
        }
        #endregion
        #endregion

        #region 异步方法
        #region CreateIndexAsync
        /// <summary>
        /// 创建索引
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="property">属性</param>
        /// <param name="collection">mongo集合</param>
        /// <param name="father">父级字段</param>
        /// <returns></returns>
        public async Task CreateIndexAsync<T>(PropertyInfo property, IMongoCollection<T> collection, string father)
        {
            foreach (var prop in property.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                //判断是否有索引
                var customAttributes = prop.GetCustomAttributes(typeof(MongoIndexAttribute), false);
                if (customAttributes?.Length > 0 && customAttributes.FirstOrDefault() is MongoIndexAttribute mongoIndex)
                {
                    var name = (string.IsNullOrWhiteSpace(father) ? prop.Name : $"{father}.{prop.Name}");
                    var keys = mongoIndex.Ascending ?
                            Builders<T>.IndexKeys.Ascending(name) :
                            Builders<T>.IndexKeys.Descending(name);
                    var model = new CreateIndexModel<T>(keys, new CreateIndexOptions
                    {
                        Name = mongoIndex.Name,
                        Unique = mongoIndex.Unique
                    });
                    await collection.Indexes.CreateOneAsync(model);
                }
                //实体
                else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    await this.CreateIndexAsync(prop, collection, (string.IsNullOrWhiteSpace(father) ? prop.Name : $"{father}.{prop.Name}"));
                }
            }
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        public async Task CreateIndexAsync<T>(string collectionName = null)
        {
            var collection = this.database.GetCollection<T>(collectionName ?? typeof(T).Name);
            var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in props)
            {
                //判断是否有索引
                var customAttributes = prop.GetCustomAttributes(typeof(MongoIndexAttribute), false);
                if (customAttributes?.Length > 0 && customAttributes.FirstOrDefault() is MongoIndexAttribute mongoIndex)
                {
                    var keys = mongoIndex.Ascending ?
                            Builders<T>.IndexKeys.Ascending(prop.Name) :
                            Builders<T>.IndexKeys.Descending(prop.Name);
                    var model = new CreateIndexModel<T>(keys, new CreateIndexOptions
                    {
                        Name = mongoIndex.Name,
                        Unique = mongoIndex.Unique
                    });
                    await collection.Indexes.CreateOneAsync(model);
                }
                //实体
                else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    await this.CreateIndexAsync(prop, collection, prop.Name);
                }
            }
        }
        #endregion

        #region DropIndexAsync
        /// <summary>
        /// 删除索引
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="indexName">索引名称</param>
        /// <param name="collectionName">表名</param>
        public async Task DropIndexAsync<T>(string indexName, string collectionName = null)
        {
            var collection = this.database.GetCollection<T>(collectionName ?? typeof(T).Name);
            await collection.Indexes.DropOneAsync(indexName);
        }
        #endregion
        #endregion
        #endregion

        #region 新增
        #region 同步方法
        #region InsertOne
        /// <summary>
        /// 插入单条新数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="entity">插入实体</param>
        public void InsertOne<T>(T entity)
        {
            this.InsertOne(typeof(T).Name, entity);
        }

        /// <summary>
        /// 插入单条新数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="entity">插入实体</param>
        public void InsertOne<T>(string collectionName, T entity)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            collection.InsertOne(entity);
            this.CreateIndex<T>(collectionName);
        }
        #endregion

        #region InsertMany
        /// <summary>
        /// 插入多条新数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="entity">插入实体</param>
        public void InsertMany<T>(List<T> entity)
        {
            this.InsertMany(typeof(T).Name, entity);
        }

        /// <summary>
        /// 插入多条新数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="entity">插入实体</param>
        public void InsertMany<T>(string collectionName, List<T> entity)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            collection.InsertMany(entity);
            this.CreateIndex<T>(collectionName);
        }
        #endregion
        #endregion

        #region 异步方法
        #region InsertOneAsync
        /// <summary>
        /// 插入单条新数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="entity">插入实体</param>
        public async Task InsertOneAsync<T>(T entity)
        {
            await this.InsertOneAsync(typeof(T).Name, entity);
        }

        /// <summary>
        /// 插入单条新数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="entity">插入实体</param>
        public async Task InsertOneAsync<T>(string collectionName, T entity)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            await collection.InsertOneAsync(entity);
            await this.CreateIndexAsync<T>(collectionName);
        }
        #endregion

        #region InsertManyAsync
        /// <summary>
        /// 插入多条新数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="entity">插入实体</param>
        public async Task InsertManyAsync<T>(List<T> entity)
        {
            await this.InsertManyAsync(typeof(T).Name, entity);
        }

        /// <summary>
        /// 插入多条新数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="entity">插入实体</param>
        public async Task InsertManyAsync<T>(string collectionName, List<T> entity)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            await collection.InsertManyAsync(entity);
            await this.CreateIndexAsync<T>(collectionName);
        }
        #endregion
        #endregion
        #endregion

        #region 删除
        #region 同步方法
        #region DeleteOne
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回是否删除成功</returns>
        public bool DeleteOne<T>(Expression<Func<T, bool>> filter, DeleteOptions options = null)
        {
            return this.DeleteOne(typeof(T).Name, filter, options);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回是否删除成功</returns>
        public bool DeleteOne<T>(string collectionName, Expression<Func<T, bool>> filter, DeleteOptions options = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return collection.DeleteOne(filter, options).DeletedCount > 0;
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回是否删除成功</returns>
        public bool DeleteOneByDefinition<T>(FilterDefinition<T> filter, DeleteOptions options = null)
        {
            return this.DeleteOneByDefinition(typeof(T).Name, filter, options);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回是否删除成功</returns>
        public bool DeleteOneByDefinition<T>(string collectionName, FilterDefinition<T> filter, DeleteOptions options = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return collection.DeleteOne(filter, options).DeletedCount > 0;
        }
        #endregion

        #region DeleteMany
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回是否删除成功</returns>
        public bool DeleteMany<T>(Expression<Func<T, bool>> filter, DeleteOptions options = null)
        {
            return this.DeleteMany(typeof(T).Name, filter, options);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回是否删除成功</returns>
        public bool DeleteMany<T>(string collectionName, Expression<Func<T, bool>> filter, DeleteOptions options = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return collection.DeleteMany(filter, options).DeletedCount > 0;
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回是否删除成功</returns>
        public bool DeleteManyByDefinition<T>(FilterDefinition<T> filter, DeleteOptions options = null)
        {
            return this.DeleteManyByDefinition(typeof(T).Name, filter, options);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回是否删除成功</returns>
        public bool DeleteManyByDefinition<T>(string collectionName, FilterDefinition<T> filter, DeleteOptions options = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return collection.DeleteMany(filter, options).DeletedCount > 0;
        }
        #endregion

        #region DropCollection
        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="collectionName">表名</param>
        public void DropCollection(string collectionName)
        {
            this.database.DropCollection(collectionName);
        }
        #endregion

        #region DropDatabase
        /// <summary>
        /// 删除数据库
        /// </summary>
        /// <param name="databaseName">数据库名称</param>
        public void DropDatabase(string databaseName)
        {
            this.client.DropDatabase(databaseName);
        }
        #endregion
        #endregion

        #region 异步方法
        #region DeleteOneAsync
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回是否删除成功</returns>
        public async Task<DeleteResult> DeleteOneAsync<T>(Expression<Func<T, bool>> filter, DeleteOptions options = null)
        {
            return await this.DeleteOneAsync(typeof(T).Name, filter, options);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回是否删除成功</returns>
        public async Task<DeleteResult> DeleteOneAsync<T>(string collectionName, Expression<Func<T, bool>> filter, DeleteOptions options = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return await collection.DeleteOneAsync(filter, options);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回是否删除成功</returns>
        public async Task<DeleteResult> DeleteOneByDefinitionAsync<T>(FilterDefinition<T> filter, DeleteOptions options = null)
        {
            return await this.DeleteOneByDefinitionAsync(typeof(T).Name, filter, options);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回是否删除成功</returns>
        public async Task<DeleteResult> DeleteOneByDefinitionAsync<T>(string collectionName, FilterDefinition<T> filter, DeleteOptions options = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return await collection.DeleteOneAsync(filter, options);
        }
        #endregion

        #region DeleteManyAsync
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回是否删除成功</returns>
        public async Task<DeleteResult> DeleteManyAsync<T>(Expression<Func<T, bool>> filter, DeleteOptions options = null)
        {
            return await this.DeleteManyAsync(typeof(T).Name, filter, options);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回是否删除成功</returns>
        public async Task<DeleteResult> DeleteManyAsync<T>(string collectionName, Expression<Func<T, bool>> filter, DeleteOptions options = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return await collection.DeleteManyAsync(filter, options);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回是否删除成功</returns>
        public async Task<DeleteResult> DeleteManyByDefinitionAsync<T>(FilterDefinition<T> filter, DeleteOptions options = null)
        {
            return await this.DeleteManyByDefinitionAsync(typeof(T).Name, filter, options);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回是否删除成功</returns>
        public async Task<DeleteResult> DeleteManyByDefinitionAsync<T>(string collectionName, FilterDefinition<T> filter, DeleteOptions options = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return await collection.DeleteManyAsync(filter, options);
        }
        #endregion

        #region DropCollectionAsync
        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="collectionName">表名</param>
        public async Task DropCollectionAsync(string collectionName)
        {
            await this.database.DropCollectionAsync(collectionName);
        }
        #endregion

        #region DropDatabaseAsync
        /// <summary>
        /// 删除数据库
        /// </summary>
        /// <param name="databaseName">数据库名称</param>
        /// <returns></returns>
        public async Task DropDatabaseAsync(string databaseName)
        {
            await this.client.DropDatabaseAsync(databaseName);
        }
        #endregion
        #endregion
        #endregion

        #region 更新
        #region 同步方法  
        #region UpdatePushItem
        /// <summary>
        /// 新增集合子项
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="item">要新增的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns>返回是否新增成功</returns>
        public bool UpdatePushItem<T>(object item, Expression<Func<T, bool>> filter)
        {
            return this.UpdatePushItem(typeof(T).Name, item, filter);
        }

        /// <summary>
        /// 新增集合子项
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="collectionName">表名</param>
        /// <param name="item">要新增的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns>返回是否新增成功</returns>
        public bool UpdatePushItem<T>(string collectionName, object item, Expression<Func<T, bool>> filter)
        {
            return this.UpdatePushItem(collectionName, GetFields<T>(item).FirstOrDefault(), item, filter);
        }

        /// <summary>
        /// 新增集合子项
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="collectionName">表名</param>
        /// <param name="field">子项名称</param>
        /// <param name="item">要新增的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns>返回是否新增成功</returns>
        public bool UpdatePushItem<T>(string collectionName, string field, object item, Expression<Func<T, bool>> filter)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return collection.UpdateOne<T>(filter, Builders<T>.Update.Push(field, item)).ModifiedCount > 0;
        }
        #endregion

        #region UpdatePullItem
        /// <summary>
        /// 删除集合子项
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="item">要删除的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns>返回是否删除成功</returns>
        public bool UpdatePullItem<T>(object item, Expression<Func<T, bool>> filter)
        {
            return this.UpdatePullItem(typeof(T).Name, item, filter);
        }

        /// <summary>
        /// 删除集合子项
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="collectionName">表名</param>
        /// <param name="item">要删除的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns>返回是否删除成功</returns>
        public bool UpdatePullItem<T>(string collectionName, object item, Expression<Func<T, bool>> filter)
        {
            return this.UpdatePushItem(collectionName, GetFields<T>(item).FirstOrDefault(), item, filter);
        }

        /// <summary>
        /// 删除集合子项
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="collectionName">表名</param>
        /// <param name="field">子项名称</param>
        /// <param name="item">要删除的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns>返回是否删除成功</returns>
        public bool UpdatePullItem<T>(string collectionName, string field, object item, Expression<Func<T, bool>> filter)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return collection.UpdateOne<T>(filter, Builders<T>.Update.Pull(field, item)).ModifiedCount > 0;
        }
        #endregion

        #region UpdateOne
        /// <summary>
        /// 更新单条操作
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="filter">条件</param>
        /// <param name="entity">新实体</param>
        /// <returns>返回是否更新成功</returns>
        public bool UpdateOne<T>(Expression<Func<T, bool>> filter, T entity)
        {
            return this.UpdateOne(typeof(T).Name, filter, entity);
        }

        /// <summary>
        /// 更新单条操作
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="entity">新实体</param>
        /// <returns>返回是否更新成功</returns>
        public bool UpdateOne<T>(string collectionName, Expression<Func<T, bool>> filter, T entity)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            var updateList = BuildUpdateDefinition<T>(entity);
            return collection.UpdateOne<T>(filter, Builders<T>.Update.Combine(updateList)).ModifiedCount > 0;
        }

        /// <summary>
        /// 更新单条操作
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="filter">条件</param>
        /// <param name="parameters">要修改的参数</param>
        /// <returns>返回是否更新成功</returns>
        public bool UpdateOneByDefinition<T>(FilterDefinition<T> filter, Dictionary<string, object> parameters)
        {
            return this.UpdateOneByDefinition(typeof(T).Name, filter, parameters);
        }

        /// <summary>
        /// 更新单条操作
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="parameters">要修改的参数</param>
        /// <returns>返回是否更新成功</returns>
        public bool UpdateOneByDefinition<T>(string collectionName, FilterDefinition<T> filter, Dictionary<string, object> parameters)
        {
            var list = new List<UpdateDefinition<T>>();
            foreach (var item in typeof(T).GetType().GetProperties())
            {
                if (!parameters.ContainsKey(item.Name)) continue;
                list.Add(Builders<T>.Update.Set(item.Name, parameters[item.Name]));
            }
            var update = Builders<T>.Update.Combine(list);
            var collection = this.database.GetCollection<T>(collectionName);
            return collection.UpdateOne(filter, update).ModifiedCount > 0;
        }
        #endregion

        #region UpdateMany
        /// <summary>
        /// 更新多条操作
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="filter">条件</param>
        /// <param name="entity">新实体</param>
        /// <returns>返回是否更新成功</returns>
        public bool UpdateMany<T>(Expression<Func<T, bool>> filter, T entity)
        {
            return this.UpdateMany(typeof(T).Name, filter, entity);
        }

        /// <summary>
        /// 更新多条操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="entity">新实体</param>
        /// <returns>返回是否更新成功</returns>
        public bool UpdateMany<T>(string collectionName, Expression<Func<T, bool>> filter, T entity)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            var updateList = BuildUpdateDefinition<T>(entity);
            return collection.UpdateMany<T>(filter, Builders<T>.Update.Combine(updateList)).ModifiedCount > 0;
        }

        /// <summary>
        /// 更新多条操作
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="filter">条件</param>
        /// <param name="parameters">要修改的参数</param>
        /// <returns>返回是否更新成功</returns>
        public bool UpdateManyByDefinition<T>(FilterDefinition<T> filter, Dictionary<string, object> parameters)
        {
            return this.UpdateManyByDefinition(typeof(T).Name, filter, parameters);
        }

        /// <summary>
        /// 更新多条操作
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="parameters">要修改的参数</param>
        /// <returns>返回是否更新成功</returns>
        public bool UpdateManyByDefinition<T>(string collectionName, FilterDefinition<T> filter, Dictionary<string, object> parameters)
        {
            var list = new List<UpdateDefinition<T>>();
            foreach (var item in typeof(T).GetType().GetProperties())
            {
                if (!parameters.ContainsKey(item.Name)) continue;
                list.Add(Builders<T>.Update.Set(item.Name, parameters[item.Name]));
            }
            var update = Builders<T>.Update.Combine(list);
            var collection = this.database.GetCollection<T>(collectionName);
            return collection.UpdateMany(filter, update).ModifiedCount > 0;
        }
        #endregion
        #endregion

        #region 异步方法
        #region UpdatePushItemAsync
        /// <summary>
        /// 新增集合子项
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>                
        /// <param name="item">要新增的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns>返回更新结果</returns>
        public async Task<UpdateResult> UpdatePushItemAsync<T>(object item, Expression<Func<T, bool>> filter)
        {
            return await this.UpdatePushItemAsync(typeof(T).Name, item, filter);
        }

        /// <summary>
        /// 新增集合子项
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="collectionName">表名</param>
        /// <param name="item">要新增的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns>返回更新结果</returns>
        public async Task<UpdateResult> UpdatePushItemAsync<T>(string collectionName, object item, Expression<Func<T, bool>> filter)
        {
            return await this.UpdatePushItemAsync(collectionName, GetFields<T>(item).FirstOrDefault(), item, filter);
        }

        /// <summary>
        /// 新增集合子项
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="collectionName">表名</param>
        /// <param name="field">子项名称</param>
        /// <param name="item">要新增的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns>返回更新结果</returns>
        public async Task<UpdateResult> UpdatePushItemAsync<T>(string collectionName, string field, object item, Expression<Func<T, bool>> filter)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return await collection.UpdateOneAsync<T>(filter, Builders<T>.Update.Push(field, item));
        }
        #endregion

        #region UpdatePullItemAsync
        /// <summary>
        /// 删除集合子项
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="item">要删除的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns>返回更新结果</returns>
        public async Task<UpdateResult> UpdatePullItemAsync<T>(object item, Expression<Func<T, bool>> filter)
        {
            return await this.UpdatePullItemAsync(typeof(T).Name, item, filter);
        }

        /// <summary>
        /// 删除集合子项
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="collectionName">表名</param>
        /// <param name="item">要删除的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns>返回更新结果</returns>
        public async Task<UpdateResult> UpdatePullItemAsync<T>(string collectionName, object item, Expression<Func<T, bool>> filter)
        {
            return await this.UpdatePullItemAsync(collectionName, GetFields<T>(item).FirstOrDefault(), item, filter);
        }

        /// <summary>
        /// 删除集合子项
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="collectionName">表名</param>
        /// <param name="field">子项名称</param>
        /// <param name="item">要删除的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns>返回更新结果</returns>
        public async Task<UpdateResult> UpdatePullItemAsync<T>(string collectionName, string field, object item, Expression<Func<T, bool>> filter)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return await collection.UpdateOneAsync<T>(filter, Builders<T>.Update.Pull(field, item));
        }
        #endregion

        #region UpdateOneAsync
        /// <summary>
        /// 更新单条操作
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="filter">条件</param>
        /// <param name="entity">新实体</param>
        /// <returns>返回更新结果</returns>
        public async Task<UpdateResult> UpdateOneAsync<T>(Expression<Func<T, bool>> filter, T entity)
        {
            return await this.UpdateOneAsync(typeof(T).Name, filter, entity);
        }

        /// <summary>
        /// 更新单条操作
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="entity">新实体</param>
        /// <returns>返回更新结果</returns>
        public async Task<UpdateResult> UpdateOneAsync<T>(string collectionName, Expression<Func<T, bool>> filter, T entity)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            var updateList = BuildUpdateDefinition<T>(entity);
            return await collection.UpdateOneAsync<T>(filter, Builders<T>.Update.Combine(updateList));
        }

        /// <summary>
        /// 更新单条操作
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="filter">条件</param>
        /// <param name="parameters">要修改的参数</param>
        /// <returns>返回更新结果</returns>
        public async Task<UpdateResult> UpdateOneByDefinitionAsync<T>(FilterDefinition<T> filter, Dictionary<string, object> parameters)
        {
            return await this.UpdateOneByDefinitionAsync(typeof(T).Name, filter, parameters);
        }

        /// <summary>
        /// 更新单条操作
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="parameters">要修改的参数</param>
        /// <returns>返回更新结果</returns>
        public async Task<UpdateResult> UpdateOneByDefinitionAsync<T>(string collectionName, FilterDefinition<T> filter, Dictionary<string, object> parameters)
        {
            var list = new List<UpdateDefinition<T>>();
            foreach (var item in typeof(T).GetType().GetProperties())
            {
                if (!parameters.ContainsKey(item.Name)) continue;
                list.Add(Builders<T>.Update.Set(item.Name, parameters[item.Name]));
            }
            var update = Builders<T>.Update.Combine(list);
            var collection = this.database.GetCollection<T>(collectionName);
            return await collection.UpdateOneAsync(filter, update);
        }
        #endregion

        #region UpdateManyAsync
        /// <summary>
        /// 更新多条操作
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="filter">条件</param>
        /// <param name="entity">新实体</param>
        /// <returns>返回更新结果</returns>
        public async Task<UpdateResult> UpdateManyAsync<T>(Expression<Func<T, bool>> filter, T entity)
        {
            return await this.UpdateManyAsync(typeof(T).Name, filter, entity);
        }

        /// <summary>
        /// 更新多条操作
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="entity">新实体</param>
        /// <returns>返回更新结果</returns>
        public async Task<UpdateResult> UpdateManyAsync<T>(string collectionName, Expression<Func<T, bool>> filter, T entity)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            var updateList = BuildUpdateDefinition<T>(entity);
            return await collection.UpdateManyAsync<T>(filter, Builders<T>.Update.Combine(updateList));
        }

        /// <summary>
        /// 更新多条操作
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="filter">条件</param>
        /// <param name="parameters">要修改的参数</param>
        /// <returns>返回更新结果</returns>
        public async Task<UpdateResult> UpdateManyByDefinitionAsync<T>(FilterDefinition<T> filter, Dictionary<string, object> parameters)
        {
            return await this.UpdateManyByDefinitionAsync(typeof(T).Name, filter, parameters);
        }

        /// <summary>
        /// 更新多条操作
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="parameters">要修改的参数</param>
        /// <returns>返回更新结果</returns>
        public async Task<UpdateResult> UpdateManyByDefinitionAsync<T>(string collectionName, FilterDefinition<T> filter, Dictionary<string, object> parameters)
        {
            var list = new List<UpdateDefinition<T>>();
            foreach (var item in typeof(T).GetType().GetProperties())
            {
                if (!parameters.ContainsKey(item.Name)) continue;
                list.Add(Builders<T>.Update.Set(item.Name, parameters[item.Name]));
            }
            var update = Builders<T>.Update.Combine(list);
            var collection = this.database.GetCollection<T>(collectionName);
            return await collection.UpdateManyAsync(filter, update);
        }
        #endregion
        #endregion
        #endregion

        #region 查询
        #region 同步方法
        #region FindEntity
        /// <summary>
        /// 查询实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>      
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回查询结果实体</returns>
        public T FindEntity<T>(Expression<Func<T, bool>> filter, FindOptions options = null)
        {
            return this.FindEntity(typeof(T).Name, filter, options);
        }

        /// <summary>
        /// 查询实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回查询结果实体</returns>
        public T FindEntity<T>(string collectionName, Expression<Func<T, bool>> filter, FindOptions options = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return collection.Find(filter ?? FilterDefinition<T>.Empty, options).Skip(0).Limit(1).FirstOrDefault();
        }

        /// <summary>
        /// 查询实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>      
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回查询结果实体</returns>
        public T FindEntityByDefinition<T>(FilterDefinition<T> filter, FindOptions options = null)
        {
            return this.FindEntityByDefinition(typeof(T).Name, filter, options);
        }

        /// <summary>
        /// 查询实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回查询结果实体</returns>
        public T FindEntityByDefinition<T>(string collectionName, FilterDefinition<T> filter, FindOptions options = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return collection.Find(filter ?? FilterDefinition<T>.Empty, options).Skip(0).Limit(1).FirstOrDefault();
        }
        #endregion

        #region FindList
        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="filter">条件</param>
        /// <param name="sort">排序</param>
        /// <param name="isDesc">是否降序</param>
        /// <param name="options">配置</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页行数</param>
        /// <returns>返回查询结果集合和总条数，默认不分页，返回总条数为0</returns>
        public (List<T> list, long total) FindList<T>(Expression<Func<T, bool>> filter = null, Expression<Func<T, object>> sort = null, bool isDesc = false, FindOptions options = null, int? pageIndex = null, int? pageSize = null)
        {
            return this.FindList(typeof(T).Name, filter, sort, isDesc, options, pageIndex, pageSize);
        }

        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="sort">排序</param>
        /// <param name="isDesc">是否降序</param>
        /// <param name="options">配置</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页行数</param>
        /// <returns>返回查询结果集合和总条数，默认不分页，返回总条数为0</returns>
        public (List<T> list, long total) FindList<T>(string collectionName, Expression<Func<T, bool>> filter = null, Expression<Func<T, object>> sort = null, bool isDesc = false, FindOptions options = null, int? pageIndex = null, int? pageSize = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            //是否分页
            if (pageIndex != null && pageSize != null)
            {
                var total = collection.CountDocuments(filter ?? FilterDefinition<T>.Empty);
                var list = sort != null ?
                    (
                        !isDesc ?
                        collection.Find(filter ?? FilterDefinition<T>.Empty, options).SortBy(sort).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToList() :
                        collection.Find(filter ?? FilterDefinition<T>.Empty, options).SortByDescending(sort).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToList()
                    ) :
                    collection.Find(filter ?? FilterDefinition<T>.Empty, options).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToList();
                return (list, total);
            }
            else
            {
                var list = sort != null ?
                    (
                        !isDesc ?
                        collection.Find(filter ?? FilterDefinition<T>.Empty, options).SortBy(sort).ToList() :
                        collection.Find(filter ?? FilterDefinition<T>.Empty, options).SortByDescending(sort).ToList()
                    ) :
                    collection.Find(filter ?? FilterDefinition<T>.Empty, options).ToList();
                return (list, 0);
            }
        }

        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="filter">条件</param>
        /// <param name="sort">排序</param>
        /// <param name="options">配置</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页行数</param>
        /// <returns>返回查询结果集合和总条数，默认不分页，返回总条数为0</returns>
        public (List<T> list, long total) FindListByDefinition<T>(FilterDefinition<T> filter = null, SortDefinition<T> sort = null, FindOptions options = null, int? pageIndex = null, int? pageSize = null)
        {
            return this.FindListByDefinition(typeof(T).Name, sort, options, pageIndex, pageSize);
        }

        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="sort">排序</param>
        /// <param name="options">配置</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页行数</param>
        /// <returns>返回查询结果集合和总条数，默认不分页，返回总条数为0</returns>
        public (List<T> list, long total) FindListByDefinition<T>(string collectionName, FilterDefinition<T> filter = null, SortDefinition<T> sort = null, FindOptions options = null, int? pageIndex = null, int? pageSize = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            //是否分页
            if (pageIndex != null && pageSize != null)
            {
                var total = collection.CountDocuments(filter ?? FilterDefinition<T>.Empty);
                var list = sort != null ?
                    collection.Find(filter ?? FilterDefinition<T>.Empty, options).Sort(sort).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToList() :
                    collection.Find(filter ?? FilterDefinition<T>.Empty, options).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToList();
                return (list, total);
            }
            else
            {
                var list = sort != null ?
                    collection.Find(filter ?? FilterDefinition<T>.Empty, options).Sort(sort).ToList() :
                    collection.Find(filter ?? FilterDefinition<T>.Empty, options).ToList();
                return (list, 0);
            }
        }
        #endregion
        #endregion

        #region 异步方法
        #region FindEntityAsync
        /// <summary>
        /// 查询实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回查询结果实体</returns>
        public async Task<T> FindEntityAsync<T>(Expression<Func<T, bool>> filter, FindOptions options = null)
        {
            return await this.FindEntityAsync(typeof(T).Name, filter, options);
        }

        /// <summary>
        /// 查询实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回查询结果实体</returns>
        public async Task<T> FindEntityAsync<T>(string collectionName, Expression<Func<T, bool>> filter, FindOptions options = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return await collection.Find(filter ?? FilterDefinition<T>.Empty, options).Skip(0).Limit(1).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 查询实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>      
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回查询结果实体</returns>
        public async Task<T> FindEntityByDefinitionAsync<T>(FilterDefinition<T> filter, FindOptions options = null)
        {
            return await this.FindEntityByDefinitionAsync(typeof(T).Name, filter, options);
        }

        /// <summary>
        /// 查询实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="options">配置</param>
        /// <returns>返回查询结果实体</returns>
        public async Task<T> FindEntityByDefinitionAsync<T>(string collectionName, FilterDefinition<T> filter, FindOptions options = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return await collection.Find(filter ?? FilterDefinition<T>.Empty, options).Skip(0).Limit(1).FirstOrDefaultAsync();
        }
        #endregion

        #region FindListAsync
        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="filter">条件</param>
        /// <param name="sort">排序</param>
        /// <param name="isDesc">是否降序</param>
        /// <param name="options">配置</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页行数</param>
        /// <returns>返回查询结果集合和总条数，默认不分页，返回总条数为0</returns>
        public async Task<(List<T> list, long total)> FindListAsync<T>(Expression<Func<T, bool>> filter = null, Expression<Func<T, object>> sort = null, bool isDesc = false, FindOptions options = null, int? pageIndex = null, int? pageSize = null)
        {
            return await this.FindListAsync(typeof(T).Name, filter, sort, isDesc, options, pageIndex, pageSize);
        }

        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="sort">排序</param>
        /// <param name="isDesc">是否降序</param>
        /// <param name="options">配置</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页行数</param>
        /// <returns>返回查询结果集合和总条数，默认不分页，返回总条数为0</returns>
        public async Task<(List<T> list, long total)> FindListAsync<T>(string collectionName, Expression<Func<T, bool>> filter = null, Expression<Func<T, object>> sort = null, bool isDesc = false, FindOptions options = null, int? pageIndex = null, int? pageSize = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            //是否分页
            if (pageIndex != null && pageSize != null)
            {
                var total = await collection.CountDocumentsAsync(filter ?? FilterDefinition<T>.Empty);
                var list = sort != null ?
                    (
                        !isDesc ?
                        await collection.Find(filter ?? FilterDefinition<T>.Empty, options).SortBy(sort).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync() :
                        await collection.Find(filter ?? FilterDefinition<T>.Empty, options).SortByDescending(sort).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync()
                    ) :
                    await collection.Find(filter ?? FilterDefinition<T>.Empty, options).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();
                return (list, total);
            }
            else
            {
                var list = sort != null ?
                    (
                        !isDesc ?
                        await collection.Find(filter ?? FilterDefinition<T>.Empty, options).SortBy(sort).ToListAsync() :
                        await collection.Find(filter ?? FilterDefinition<T>.Empty, options).SortByDescending(sort).ToListAsync()
                    ) :
                    await collection.Find(filter ?? FilterDefinition<T>.Empty, options).ToListAsync();
                return (list, 0);
            }
        }

        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="filter">条件</param>
        /// <param name="sort">排序</param>
        /// <param name="options">配置</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页行数</param>
        /// <returns>返回查询结果集合和总条数，默认不分页，返回总条数为0</returns>
        public async Task<(List<T> list, long total)> FindListByDefinitionAsync<T>(FilterDefinition<T> filter = null, SortDefinition<T> sort = null, FindOptions options = null, int? pageIndex = null, int? pageSize = null)
        {
            return await this.FindListByDefinitionAsync(typeof(T).Name, sort, options, pageIndex, pageSize);
        }

        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="sort">排序</param>
        /// <param name="options">配置</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页行数</param>
        /// <returns>返回查询结果集合和总条数，默认不分页，返回总条数为0</returns>
        public async Task<(List<T> list, long total)> FindListByDefinitionAsync<T>(string collectionName, FilterDefinition<T> filter = null, SortDefinition<T> sort = null, FindOptions options = null, int? pageIndex = null, int? pageSize = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            //是否分页
            if (pageIndex != null && pageSize != null)
            {
                var total = await collection.CountDocumentsAsync(filter ?? FilterDefinition<T>.Empty);
                var list = sort != null ?
                    await collection.Find(filter ?? FilterDefinition<T>.Empty, options).Sort(sort).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync() :
                    await collection.Find(filter ?? FilterDefinition<T>.Empty, options).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();
                return (list, total);
            }
            else
            {
                var list = sort != null ?
                    await collection.Find(filter ?? FilterDefinition<T>.Empty, options).Sort(sort).ToListAsync() :
                    await collection.Find(filter ?? FilterDefinition<T>.Empty, options).ToListAsync();
                return (list, 0);
            }
        }
        #endregion
        #endregion
        #endregion
    }

    /// <summary>  
    /// MongoDB索引
    /// </summary>  
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MongoIndexAttribute : Attribute
    {
        /// <summary>
        /// 索引名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>  
        /// 是否唯一索引，默认false
        /// </summary>  
        public bool Unique { get; set; }

        /// <summary>  
        /// 是否升序，默认true  
        /// </summary>  
        public bool Ascending { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">索引名称</param>
        /// <param name="unique">是否唯一索引</param>
        /// <param name="ascding">是否升序</param>
        public MongoIndexAttribute(string name, bool unique = false, bool ascding = true)
        {
            this.Name = name;
            this.Unique = unique;
            this.Ascending = ascding;
        }
    }

    /// <summary>
    /// MongoDB实体基类
    /// </summary>
    [BsonIgnoreExtraElements(Inherited = true)]
    public abstract class MongodbBaseEntity
    {
        /// <summary>
        /// 主键id
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
    }
}