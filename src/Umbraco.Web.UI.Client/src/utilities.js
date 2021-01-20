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
    const copy = (src, dst) => angular.copy(src, dst);

    /**
     * Equivalent to angular.isArray
     */
    const isArray = val => Array.isArray(val) || val instanceof Array;

    /**
     * Facade to angular.equals
     */
    const equals = (a, b) => angular.equals(a, b);
    
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
        } else if (value &&  window.document === value) {
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

    /**
     * Equivalent to angular.fromJson
     */
    const fromJson = (val) => {
        if (!isString(val)) {
            return val;
        }
        return JSON.parse(val);
    }

    /**
     * Not equivalent to angular.forEach. But like the angularJS method this does not fail on null or undefined.
     */
    const forEach = (obj, iterator) => {
        if (obj) {
            return obj.forEach(iterator);
        }
        return obj;
    }

    let _utilities = {
        noop: noop,
        copy: copy,
        isArray: isArray,
        equals: equals,
        extend: extend,
        isFunction: isFunction,
        isUndefined: isUndefined,
        isDefined: isDefined,
        isString: isString,
        isNumber: isNumber,
        isObject: isObject,
        fromJson: fromJson,
        toJson: toJson,
        forEach: forEach
    };

    if (typeof (window.Utilities) === 'undefined') {
        window.Utilities = _utilities;
    }
})(window);
