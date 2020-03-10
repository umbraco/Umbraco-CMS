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
        isObject: isObject
    };

    if (typeof (window.Utilities) === 'undefined') {
        window.Utilities = _utilities;
    }
})(window);