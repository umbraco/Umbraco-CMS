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
     * Equivalent to angular.copy
     * Abstraction of underscore.clone
     */
    const copy = val => _.clone(val);

    /**
     * Equivalent to angular.isArray
     */
    const isArray = val => Array.isArray(val) || val instanceof Array;

    /**
     * Equivalent to angular.equals
     * Abstraction of underscore.isEqual
     */
    const equals = (a, b) => _.isEqual(a, b);

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