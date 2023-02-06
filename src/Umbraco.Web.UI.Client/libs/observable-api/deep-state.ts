import { BehaviorSubject } from "rxjs";
import { createObservablePart } from "./create-observable-part.method";


// TODO: Should this handle array as well?
function deepFreeze<T>(inObj: T): T {
	if(inObj != null && typeof inObj === 'object') {
		Object.freeze(inObj);

		Object.getOwnPropertyNames(inObj)?.forEach(function (prop) {
			// eslint-disable-next-line no-prototype-builtins
			if ((inObj as any).hasOwnProperty(prop)
				&& (inObj as any)[prop] != null
				&& typeof (inObj as any)[prop] === 'object'
				&& !Object.isFrozen((inObj as any)[prop])) {
					deepFreeze((inObj as any)[prop]);
				}
		});
	}
	return inObj;
}


export function naiveObjectComparison(objOne: any, objTwo: any): boolean {
	return JSON.stringify(objOne) === JSON.stringify(objTwo);
}





export type MappingFunction<T, R> = (mappable: T) => R;
export type MemoizationFunction<R> = (previousResult: R, currentResult: R) => boolean;

export function defaultMemoization(previousValue: any, currentValue: any): boolean {
	if (typeof previousValue === 'object' && typeof currentValue === 'object') {
		return naiveObjectComparison(previousValue, currentValue);
	}
	return previousValue === currentValue;
}

/**
 * @export
 * @class DeepState
 * @extends {BehaviorSubject<T>}
 * @description - A RxJS BehaviorSubject which deepFreezes the data to ensure its not manipulated from any implementations.
 * Additionally the Subject ensures the data is unique, not updating any Observes unless there is an actual change of the content.
 */
export class DeepState<T> extends BehaviorSubject<T> {
	constructor(initialData: T) {
		super(deepFreeze(initialData));
	}

	getObservablePart<ReturnType>(
		mappingFunction: MappingFunction<T, ReturnType>,
		memoizationFunction?: MemoizationFunction<ReturnType>
	) {
		return createObservablePart(this, mappingFunction, memoizationFunction);
	}

	next(newData: T): void {
		const frozenData = deepFreeze(newData);
		// Only update data if its different than current data.
		if (!naiveObjectComparison(frozenData, this.getValue())) {
			super.next(frozenData);
		}
	}
}
