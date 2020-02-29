/**
 * A friendly utility collection to replace AngularJs' ng-functions
 * If it doesn't exist here, it's probably available as vanilla JS
 * 
 * Still carries a dependency on lodash, but if usages of lodash from 
 * elsewhere in the codebase can instead use these methods, the lodash
 * dependency will be nicely abstracted and can be removed/swapped later
 */

var umb = umb || {};

/**
 * Equivalent to angular.noop
 */
umb.noop = () => {};

/**
 * Equivalent to angular.copy
 * Abstraction of lodasd.cloneDeep
 */
umb.copy = val => _.clone(val);

/**
 * Equivalent to angular.isArray
 */
umb.isArray = val => Array.isArray(val) || val instanceof Array;

/**
 * Equivalent to angular.equals
 * Abstraction of lodash.isEqual
 */
umb.equals = (a, b) => _.isEqual(a, b);

/**
 * Equivalent to angular.isFunction
 */
umb.isFunction = val => typeof val === 'function';

/**
 * Equivalent to angular.isUndefined
 */
umb.isUndefined = val => typeof val === 'undefined';

/**
 * Equivalent to angular.isDefined. Inverts result of umb.isUndefined
 */
umb.isDefined = val => !umb.isUndefined(val);

/**
 * Equivalent to angular.isString
 */
umb.isString = val => typeof val === 'string';

/**
 * Equivalent to angular.isNumber
 */
umb.isNumber = val => typeof val === 'number';

/**
 * Equivalent to angular.isObject
 */
umb.isObject = val => val !== null && typeof val === 'object';