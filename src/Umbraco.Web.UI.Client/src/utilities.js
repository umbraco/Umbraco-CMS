/**
 * A friendly utility collection to replace AngularJs' ng-functions
 * If it doesn't exist here, it's probably available as vanilla JS
 * 
 * Still carries a dependency on underscore, but if usages of underscore from 
 * elsewhere in the codebase can instead use these methods, the underscore
 * dependency will be nicely abstracted and can be removed/swapped later
 * 
 * This collection is open to extension...
 */
(function (window) {

  /**
   * Equivalent to angular.noop
   */
  const noop = () => { };

  /**
   * Facade to angular.copy
   */
  const copy = val => angular.copy(val);

  /**
   * Equivalent to angular.isArray
   */
  const isArray = val => Array.isArray(val) || val instanceof Array;

  /**
   * Equivalent to angular.equals
   */
  const equals = (a, b) => {
    if (o1 === o2) return true;
    if (o1 === null || o2 === null) return false;
    // eslint-disable-next-line no-self-compare
    if (o1 !== o1 && o2 !== o2) return true; // NaN === NaN
    var t1 = typeof o1, t2 = typeof o2, length, key, keySet;
    if (t1 === t2 && t1 === 'object') {
      if (isArray(o1)) {
        if (!isArray(o2)) return false;
        if ((length = o1.length) === o2.length) {
          for (key = 0; key < length; key++) {
            if (!equals(o1[key], o2[key])) return false;
          }
          return true;
        }
      } else if (isDate(o1)) {
        if (!isDate(o2)) return false;
        return simpleCompare(o1.getTime(), o2.getTime());
      } else if (isRegExp(o1)) {
        if (!isRegExp(o2)) return false;
        return o1.toString() === o2.toString();
      } else {
        if (isScope(o1) || isScope(o2) || isWindow(o1) || isWindow(o2) ||
          isArray(o2) || isDate(o2) || isRegExp(o2)) return false;
        keySet = createMap();
        for (key in o1) {
          if (key.charAt(0) === '$' || isFunction(o1[key])) continue;
          if (!equals(o1[key], o2[key])) return false;
          keySet[key] = true;
        }
        for (key in o2) {
          if (!(key in keySet) &&
            key.charAt(0) !== '$' &&
            isDefined(o2[key]) &&
            !isFunction(o2[key])) return false;
        }
        return true;
      }
    }
    return false;

  }

  /** 
   * Equivalent to angular.isDate
   */
  const isDate = val => {
    return toString.call(value) === '[object Date]';
  }

  /**
   * Equivalent to angular.isRegExp
   */
  const isRegExp = val => {
    return toString.call(val) === '[object RegExp]';
  }

  /** 
   * Equivalent to angular.simpleCompare
   */
  const simpleCompare = (a, b) => {
    return a === b || (a !== a && b !== b);
  }

  /**
   * Facade to angular.extend
   * Use this with Angular objects, for vanilla JS objects, use Object.assign()
   */
  const extend = (dst, src) => angular.extend(dst, src);

  /**
   * Equivalent to angular.isFunction
   */
  const isFunction = val => typeof val === 'function';

  /**
   * Equivalent to angular.isUndefined
   */
  const isUndefined = val => typeof val === 'undefined';

  /**
   * Equivalent to angular.isDefined. Inverts result of const isUndefined
   */
  const isDefined = val => !isUndefined(val);

  /**
   * Equivalent to angular.isString
   */
  const isString = val => typeof val === 'string';

  /**
   * Equivalent to angular.isNumber
   */
  const isNumber = val => typeof val === 'number';

  /**
   * Equivalent to angular.isObject
   */
  const isObject = val => val !== null && typeof val === 'object';

  const isWindow = obj => obj && obj.window === obj;

  const isScope = obj => obj && obj.$evalAsync && obj.$watch;

  const toJsonReplacer = (key, value) => {
    var val = value;
    if (typeof key === 'string' && key.charAt(0) === '$' && key.charAt(1) === '$') {
      val = undefined;
    } else if (isWindow(value)) {
      val = '$WINDOW';
    } else if (value && window.document === value) {
      val = '$DOCUMENT';
    } else if (isScope(value)) {
      val = '$SCOPE';
    }
    return val;
  }
  /**
   * Equivalent to angular.toJson
   */
  const toJson = (obj, pretty) => {
    if (isUndefined(obj)) return undefined;
    if (!isNumber(pretty)) {
      pretty = pretty ? 2 : null;
    }
    return JSON.stringify(obj, toJsonReplacer, pretty);
  }

  let _utilities = {
    noop: noop,
    copy: copy,
    isDate: isDate,
    isRegExp: isRegExp,
    simpleCompare: simpleCompare,
    isArray: isArray,
    equals: equals,
    extend: extend,
    isFunction: isFunction,
    isUndefined: isUndefined,
    isDefined: isDefined,
    isString: isString,
    isNumber: isNumber,
    isObject: isObject,
    toJson: toJson
  };

  if (typeof (window.Utilities) === 'undefined') {
    window.Utilities = _utilities;
  }
})(window);