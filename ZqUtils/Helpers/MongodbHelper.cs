#region License
/***
 * Copyright © 2018, 张强 (943620963@qq.com).
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
        /// <param name="connectionString">链接字符串</param>
        public MongodbHelper(string databaseName, string connectionString)
        {
            this.databaseName = databaseName;
            this.connectionString = connectionString;
            this.client = new MongoClient(this.connectionString);
            this.database = this.client.GetDatabase(this.databaseName);
        }
        #endregion

        #region 构建Mongo的更新表达式
        /// <summary>
        /// 构建Mongo的更新表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldList"></param>
        /// <param name="property"></param>
        /// <param name="propertyValue"></param>
        /// <param name="item"></param>
        /// <param name="father"></param>
        public static void BuildUpdateDefinition<T>(
              List<UpdateDefinition<T>> fieldList,
              PropertyInfo property,
              object propertyValue,
              T item,
              string father)
        {
            //复杂类型
            if (property.PropertyType.IsClass && property.PropertyType != typeof(string) && propertyValue != null)
            {
                //集合
                if (typeof(IList).IsAssignableFrom(propertyValue.GetType()))
                {
                    foreach (var sub in property.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        if (sub.PropertyType.IsClass && sub.PropertyType != typeof(string))
                        {
                            if (propertyValue is IList arr && arr.Count > 0)
                            {
                                for (int index = 0; index < arr.Count; index++)
                                {
                                    foreach (var subInner in sub.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                                    {
                                        if (string.IsNullOrWhiteSpace(father))
                                            BuildUpdateDefinition(fieldList, subInner, subInner.GetValue(arr[index]), item, property.Name + ".$");
                                        else
                                            BuildUpdateDefinition(fieldList, subInner, subInner.GetValue(arr[index]), item, father + "." + property.Name + ".$");
                                    }
                                }
                            }
                        }
                    }
                }
                //实体
                else
                {
                    foreach (var sub in property.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {

                        if (string.IsNullOrWhiteSpace(father))
                            BuildUpdateDefinition(fieldList, sub, sub.GetValue(propertyValue), item, property.Name);
                        else
                            BuildUpdateDefinition(fieldList, sub, sub.GetValue(propertyValue), item, father + "." + property.Name);
                    }
                }
            }
            //简单类型
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
                        fieldList.Add(Builders<T>.Update.Set(father + "." + property.Name, propertyValue));
                    }
                }
            }
        }

        /// <summary>
        /// 构建Mongo的更新表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
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

        #region 获取子项的名称
        /// <summary>
        /// 获取子项名称
        /// </summary>        
        /// <param name="property"></param>
        /// <param name="entity"></param>
        /// <param name="father"></param>
        /// <param name="isExists"></param>
        /// <returns></returns>
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
        /// 获取子项名称集合
        /// </summary>
        /// <typeparam name="T"></typeparam>        
        /// <param name="entity"></param>
        /// <returns></returns>
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

        #region 新增
        #region 同步方法
        /// <summary>
        /// 插入单条新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>        
        /// <param name="entity">插入实体</param>
        public void InsertOne<T>(T entity)
        {
            var collection = this.database.GetCollection<T>(typeof(T).Name);
            collection.InsertOne(entity);
        }

        /// <summary>
        /// 插入单条新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="entity">插入实体</param>
        public void InsertOne<T>(string collectionName, T entity)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            collection.InsertOne(entity);
        }

        /// <summary>
        /// 插入多条新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>        
        /// <param name="entity">插入实体</param>
        public void InsertMany<T>(List<T> entity)
        {
            var collection = this.database.GetCollection<T>(typeof(T).Name);
            collection.InsertMany(entity);
        }

        /// <summary>
        /// 插入多条新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="entity">插入实体</param>
        public void InsertMany<T>(string collectionName, List<T> entity)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            collection.InsertMany(entity);
        }
        #endregion

        #region 异步方法
        /// <summary>
        /// 插入单条新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>        
        /// <param name="entity">插入实体</param>
        public async Task InsertOneAsync<T>(T entity)
        {
            var collection = this.database.GetCollection<T>(typeof(T).Name);
            await collection.InsertOneAsync(entity);
        }

        /// <summary>
        /// 插入单条新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="entity">插入实体</param>
        public async Task InsertOneAsync<T>(string collectionName, T entity)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            await collection.InsertOneAsync(entity);
        }

        /// <summary>
        /// 插入多条新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>        
        /// <param name="entity">插入实体</param>
        public async Task InsertManyAsync<T>(List<T> entity)
        {
            var collection = this.database.GetCollection<T>(typeof(T).Name);
            await collection.InsertManyAsync(entity);
        }

        /// <summary>
        /// 插入多条新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="entity">插入实体</param>
        public async Task InsertManyAsync<T>(string collectionName, List<T> entity)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            await collection.InsertManyAsync(entity);
        }
        #endregion
        #endregion

        #region 删除
        #region 同步方法
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T"></typeparam>        
        /// <param name="query"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public bool DeleteOne<T>(Expression<Func<T, bool>> query, DeleteOptions options = null)
        {
            var collection = this.database.GetCollection<T>(typeof(T).Name);
            return collection.DeleteOne(query, options).DeletedCount > 0;
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="query"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public bool DeleteOne<T>(string collectionName, Expression<Func<T, bool>> query, DeleteOptions options = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return collection.DeleteOne(query, options).DeletedCount > 0;
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T"></typeparam>        
        /// <param name="query"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public bool DeleteMany<T>(Expression<Func<T, bool>> query, DeleteOptions options = null)
        {
            var collection = this.database.GetCollection<T>(typeof(T).Name);
            return collection.DeleteMany(query, options).DeletedCount > 0;
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="query"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public bool DeleteMany<T>(string collectionName, Expression<Func<T, bool>> query, DeleteOptions options = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return collection.DeleteMany(query, options).DeletedCount > 0;
        }

        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="collectionName"></param>
        public void DropCollection(string collectionName)
        {
            this.database.DropCollection(collectionName);
        }
        #endregion

        #region 异步方法
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T"></typeparam>        
        /// <param name="query"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<DeleteResult> DeleteOneAsync<T>(Expression<Func<T, bool>> query, DeleteOptions options = null)
        {
            var collection = this.database.GetCollection<T>(typeof(T).Name);
            return await collection.DeleteOneAsync(query, options);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="query"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<DeleteResult> DeleteOneAsync<T>(string collectionName, Expression<Func<T, bool>> query, DeleteOptions options = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return await collection.DeleteOneAsync(query, options);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T"></typeparam>        
        /// <param name="query"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<DeleteResult> DeleteManyAsync<T>(Expression<Func<T, bool>> query, DeleteOptions options = null)
        {
            var collection = this.database.GetCollection<T>(typeof(T).Name);
            return await collection.DeleteManyAsync(query, options);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="query"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<DeleteResult> DeleteManyAsync<T>(string collectionName, Expression<Func<T, bool>> query, DeleteOptions options = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return await collection.DeleteManyAsync(query, options);
        }

        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="collectionName"></param>
        public async Task DropCollectionAsync(string collectionName)
        {
            await this.database.DropCollectionAsync(collectionName);
        }
        #endregion
        #endregion

        #region 更新
        #region 同步方法  
        #region UpdatePushItem
        /// <summary>
        /// 新增集合子项
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>        
        /// <param name="item">要新增的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public bool UpdatePushItem<T>(object item, Expression<Func<T, bool>> filter) where T : class
        {
            var collection = this.database.GetCollection<T>(typeof(T).Name);
            return collection.UpdateOne<T>(filter, Builders<T>.Update.Push(GetFields<T>(item).FirstOrDefault(), item)).ModifiedCount > 0;
        }

        /// <summary>
        /// 新增集合子项
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>        
        /// <param name="collectionName">表名</param>
        /// <param name="item">要新增的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public bool UpdatePushItem<T>(string collectionName, object item, Expression<Func<T, bool>> filter) where T : class
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return collection.UpdateOne<T>(filter, Builders<T>.Update.Push(GetFields<T>(item).FirstOrDefault(), item)).ModifiedCount > 0;
        }

        /// <summary>
        /// 新增集合子项
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>        
        /// <param name="collectionName">表名</param>
        /// <param name="field">子项名称</param>
        /// <param name="item">要新增的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public bool UpdatePushItem<T>(string collectionName, string field, object item, Expression<Func<T, bool>> filter) where T : class
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return collection.UpdateOne<T>(filter, Builders<T>.Update.Push(field, item)).ModifiedCount > 0;
        }
        #endregion

        #region UpdatePullItem
        /// <summary>
        /// 删除集合子项
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>        
        /// <param name="item">要删除的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public bool UpdatePullItem<T>(object item, Expression<Func<T, bool>> filter) where T : class
        {
            var collection = this.database.GetCollection<T>(typeof(T).Name);
            return collection.UpdateOne<T>(filter, Builders<T>.Update.Pull(GetFields<T>(item).FirstOrDefault(), item)).ModifiedCount > 0;
        }

        /// <summary>
        /// 删除集合子项
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>        
        /// <param name="collectionName">表名</param>
        /// <param name="item">要删除的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public bool UpdatePullItem<T>(string collectionName, object item, Expression<Func<T, bool>> filter) where T : class
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return collection.UpdateOne<T>(filter, Builders<T>.Update.Pull(GetFields<T>(item).FirstOrDefault(), item)).ModifiedCount > 0;
        }

        /// <summary>
        /// 删除集合子项
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>        
        /// <param name="collectionName">表名</param>
        /// <param name="field">子项名称</param>
        /// <param name="item">要删除的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public bool UpdatePullItem<T>(string collectionName, string field, object item, Expression<Func<T, bool>> filter) where T : class
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return collection.UpdateOne<T>(filter, Builders<T>.Update.Pull(field, item)).ModifiedCount > 0;
        }
        #endregion

        #region UpdateOne
        /// <summary>
        /// 更新单条操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>        
        /// <param name="filter">条件</param>
        /// <param name="entity">新实体</param>
        public bool UpdateOne<T>(Expression<Func<T, bool>> filter, T entity) where T : class
        {
            var collection = this.database.GetCollection<T>(typeof(T).Name);
            var updateList = BuildUpdateDefinition<T>(entity);
            return collection.UpdateOne<T>(filter, Builders<T>.Update.Combine(updateList)).ModifiedCount > 0;
        }

        /// <summary>
        /// 更新单条操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="entity">新实体</param>
        public bool UpdateOne<T>(string collectionName, Expression<Func<T, bool>> filter, T entity) where T : class
        {
            var collection = this.database.GetCollection<T>(collectionName);
            var updateList = BuildUpdateDefinition<T>(entity);
            return collection.UpdateOne<T>(filter, Builders<T>.Update.Combine(updateList)).ModifiedCount > 0;
        }
        #endregion

        #region UpdateMany
        /// <summary>
        /// 更新多条操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>        
        /// <param name="filter">条件</param>
        /// <param name="entity">新实体</param>
        public bool UpdateMany<T>(Expression<Func<T, bool>> filter, T entity)
        {
            var collection = this.database.GetCollection<T>(typeof(T).Name);
            var updateList = BuildUpdateDefinition<T>(entity);
            return collection.UpdateMany<T>(filter, Builders<T>.Update.Combine(updateList)).ModifiedCount > 0;
        }

        /// <summary>
        /// 更新多条操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="entity">新实体</param>
        public bool UpdateMany<T>(string collectionName, Expression<Func<T, bool>> filter, T entity)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            var updateList = BuildUpdateDefinition<T>(entity);
            return collection.UpdateMany<T>(filter, Builders<T>.Update.Combine(updateList)).ModifiedCount > 0;
        }
        #endregion
        #endregion

        #region 异步方法
        #region UpdatePushItemAsync
        /// <summary>
        /// 新增集合子项
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>                
        /// <param name="item">要新增的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<UpdateResult> UpdatePushItemAsync<T>(object item, Expression<Func<T, bool>> filter) where T : class
        {
            var collection = this.database.GetCollection<T>(typeof(T).Name);
            return await collection.UpdateOneAsync<T>(filter, Builders<T>.Update.Push(GetFields<T>(item).FirstOrDefault(), item));
        }

        /// <summary>
        /// 新增集合子项
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>        
        /// <param name="collectionName">表名</param>
        /// <param name="item">要新增的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<UpdateResult> UpdatePushItemAsync<T>(string collectionName, object item, Expression<Func<T, bool>> filter) where T : class
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return await collection.UpdateOneAsync<T>(filter, Builders<T>.Update.Push(GetFields<T>(item).FirstOrDefault(), item));
        }

        /// <summary>
        /// 新增集合子项
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>        
        /// <param name="collectionName">表名</param>
        /// <param name="field">子项名称</param>
        /// <param name="item">要新增的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<UpdateResult> UpdatePushItemAsync<T>(string collectionName, string field, object item, Expression<Func<T, bool>> filter) where T : class
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return await collection.UpdateOneAsync<T>(filter, Builders<T>.Update.Push(field, item));
        }
        #endregion

        #region UpdatePullItemAsync
        /// <summary>
        /// 删除集合子项
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>        
        /// <param name="item">要删除的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<UpdateResult> UpdatePullItemAsync<T>(object item, Expression<Func<T, bool>> filter) where T : class
        {
            var collection = this.database.GetCollection<T>(typeof(T).Name);
            return await collection.UpdateOneAsync<T>(filter, Builders<T>.Update.Pull(GetFields<T>(item).FirstOrDefault(), item));
        }

        /// <summary>
        /// 删除集合子项
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>        
        /// <param name="collectionName">表名</param>
        /// <param name="item">要删除的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<UpdateResult> UpdatePullItemAsync<T>(string collectionName, object item, Expression<Func<T, bool>> filter) where T : class
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return await collection.UpdateOneAsync<T>(filter, Builders<T>.Update.Pull(GetFields<T>(item).FirstOrDefault(), item));
        }

        /// <summary>
        /// 删除集合子项
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>        
        /// <param name="collectionName">表名</param>
        /// <param name="field">子项名称</param>
        /// <param name="item">要删除的子项值</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<UpdateResult> UpdatePullItemAsync<T>(string collectionName, string field, object item, Expression<Func<T, bool>> filter) where T : class
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return await collection.UpdateOneAsync<T>(filter, Builders<T>.Update.Pull(field, item));
        }
        #endregion

        #region UpdateOneAsync
        /// <summary>
        /// 更新单条操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>        
        /// <param name="filter">条件</param>
        /// <param name="entity">新实体</param>
        public async Task<UpdateResult> UpdateOneAsync<T>(Expression<Func<T, bool>> filter, T entity) where T : class
        {
            var collection = this.database.GetCollection<T>(typeof(T).Name);
            var updateList = BuildUpdateDefinition<T>(entity);
            return await collection.UpdateOneAsync<T>(filter, Builders<T>.Update.Combine(updateList));
        }

        /// <summary>
        /// 更新单条操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="entity">新实体</param>
        public async Task<UpdateResult> UpdateOneAsync<T>(string collectionName, Expression<Func<T, bool>> filter, T entity) where T : class
        {
            var collection = this.database.GetCollection<T>(collectionName);
            var updateList = BuildUpdateDefinition<T>(entity);
            return await collection.UpdateOneAsync<T>(filter, Builders<T>.Update.Combine(updateList));
        }
        #endregion

        #region UpdateManyAsync
        /// <summary>
        /// 更新多条操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>        
        /// <param name="filter">条件</param>
        /// <param name="entity">新实体</param>
        public async Task<UpdateResult> UpdateManyAsync<T>(Expression<Func<T, bool>> filter, T entity)
        {
            var collection = this.database.GetCollection<T>(typeof(T).Name);
            var updateList = BuildUpdateDefinition<T>(entity);
            return await collection.UpdateManyAsync<T>(filter, Builders<T>.Update.Combine(updateList));
        }

        /// <summary>
        /// 更新多条操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="entity">新实体</param>
        public async Task<UpdateResult> UpdateManyAsync<T>(string collectionName, Expression<Func<T, bool>> filter, T entity)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            var updateList = BuildUpdateDefinition<T>(entity);
            return await collection.UpdateManyAsync<T>(filter, Builders<T>.Update.Combine(updateList));
        }
        #endregion
        #endregion
        #endregion

        #region 查询
        #region 同步方法
        /// <summary>
        /// 查询实体
        /// </summary>
        /// <typeparam name="T"></typeparam>      
        /// <param name="filter"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public T FindEntity<T>(Expression<Func<T, bool>> filter, FindOptions options = null)
        {
            var collection = this.database.GetCollection<T>(typeof(T).Name);
            return collection.Find(filter, options).Skip(0).Limit(1).FirstOrDefault();
        }

        /// <summary>
        /// 查询实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="filter"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public T FindEntity<T>(string collectionName, Expression<Func<T, bool>> filter, FindOptions options = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return collection.Find(filter, options).Skip(0).Limit(1).FirstOrDefault();
        }

        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="T"></typeparam>        
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="isDesc"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public List<T> FindList<T>(Expression<Func<T, bool>> filter = null, Expression<Func<T, object>> sort = null, bool isDesc = false, FindOptions options = null)
        {
            var collection = this.database.GetCollection<T>(typeof(T).Name);
            if (sort == null)
                return collection.Find(filter == null ? FilterDefinition<T>.Empty : filter, options).ToList();
            if (!isDesc)
                return collection.Find(filter, options).SortBy(sort).ToList();
            else
                return collection.Find(filter, options).SortByDescending(sort).ToList();
        }

        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="isDesc"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public List<T> FindList<T>(string collectionName, Expression<Func<T, bool>> filter = null, Expression<Func<T, object>> sort = null, bool isDesc = false, FindOptions options = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            if (sort == null)
                return collection.Find(filter == null ? FilterDefinition<T>.Empty : filter, options).ToList();
            if (!isDesc)
                return collection.Find(filter, options).SortBy(sort).ToList();
            else
                return collection.Find(filter, options).SortByDescending(sort).ToList();
        }
        #endregion

        #region 异步方法
        /// <summary>
        /// 查询实体
        /// </summary>
        /// <typeparam name="T"></typeparam>        
        /// <param name="filter"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<T> FindEntityAsync<T>(Expression<Func<T, bool>> filter, FindOptions options = null)
        {
            var collection = this.database.GetCollection<T>(typeof(T).Name);
            return await collection.Find(filter, options).Skip(0).Limit(1).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 查询实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="filter"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<T> FindEntityAsync<T>(string collectionName, Expression<Func<T, bool>> filter, FindOptions options = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            return await collection.Find(filter, options).Skip(0).Limit(1).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="T"></typeparam>        
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="isDesc"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<List<T>> FindListAsync<T>(Expression<Func<T, bool>> filter = null, Expression<Func<T, object>> sort = null, bool isDesc = false, FindOptions options = null)
        {
            var collection = this.database.GetCollection<T>(typeof(T).Name);
            if (sort == null)
                return await collection.Find(filter == null ? FilterDefinition<T>.Empty : filter, options).ToListAsync();
            if (!isDesc)
                return await collection.Find(filter, options).SortBy(sort).ToListAsync();
            else
                return await collection.Find(filter, options).SortByDescending(sort).ToListAsync();
        }

        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="isDesc"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<List<T>> FindListAsync<T>(string collectionName, Expression<Func<T, bool>> filter = null, Expression<Func<T, object>> sort = null, bool isDesc = false, FindOptions options = null)
        {
            var collection = this.database.GetCollection<T>(collectionName);
            if (sort == null)
                return await collection.Find(filter == null ? FilterDefinition<T>.Empty : filter, options).ToListAsync();
            if (!isDesc)
                return await collection.Find(filter, options).SortBy(sort).ToListAsync();
            else
                return await collection.Find(filter, options).SortByDescending(sort).ToListAsync();
        }
        #endregion
        #endregion
    }
}