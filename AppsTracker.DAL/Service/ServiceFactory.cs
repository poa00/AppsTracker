﻿#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections;

using AppsTracker.Common.Utils;

namespace AppsTracker.DAL.Service
{
    public sealed class ServiceFactory
    {
        private static readonly Hashtable _hashTable = new Hashtable();

        private ServiceFactory() { }

        public static void Register<T>(Func<T> getter) where T : class, IBaseService
        {
            Ensure.NotNull(getter);
            Ensure.Condition<InvalidOperationException>(_hashTable.ContainsKey(typeof(T)) == false, string.Format("{0} is already inserted", typeof(T)));

            _hashTable.Add(typeof(T), getter);
        }

        public static T Get<T>() where T : class, IBaseService
        {
            Ensure.Condition<InvalidOperationException>(_hashTable.ContainsKey(typeof(T)), string.Format("Can't resolve {0}", typeof(T)));

            var getter = _hashTable[typeof(T)];
            Func<T> res = (Func<T>)getter;
            return res();
        }

        public static bool ContainsKey<T>() where T : class, IBaseService
        {
            return _hashTable.ContainsKey(typeof(T));
        }
    }
}
