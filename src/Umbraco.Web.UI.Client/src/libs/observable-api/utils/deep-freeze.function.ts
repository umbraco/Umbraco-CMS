export const deepFreeze = Object.freeze(function deepFreezeImpl<T>(inObj: T): T {
	if (inObj != null && typeof inObj === 'object') {
		Object.freeze(inObj);

		Object.getOwnPropertyNames(inObj)?.forEach(function (prop) {
			if (
				// eslint-disable-next-line no-prototype-builtins
				(inObj as any).hasOwnProperty(prop) &&
				(inObj as any)[prop] != null &&
				typeof (inObj as any)[prop] === 'object' &&
				!Object.isFrozen((inObj as any)[prop])
			) {
				deepFreeze((inObj as any)[prop]);
			}
		});
	}
	return inObj;
});
