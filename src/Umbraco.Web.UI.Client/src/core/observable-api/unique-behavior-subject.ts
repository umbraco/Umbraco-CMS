import { BehaviorSubject, distinctUntilChanged, map, Observable, shareReplay } from "rxjs";


function deepFreeze<T>(inObj: T): T {
    if(inObj) {
      Object.freeze(inObj);
      Object.getOwnPropertyNames(inObj).forEach(function (prop) {
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




type MappingFunction<T, R> = (mappable: T) => R;
type MemoizationFunction<R> = (previousResult: R, currentResult: R) => boolean;

function defaultMemoization(previousValue: any, currentValue: any): boolean {
  if (typeof previousValue === 'object' && typeof currentValue === 'object') {
    return naiveObjectComparison(previousValue, currentValue);
  }
  return previousValue === currentValue;
}
export function CreateObservablePart<T, R> (
	source$: Observable<T>,
	mappingFunction: MappingFunction<T, R>,
	memoizationFunction?: MemoizationFunction<R>
): Observable<R> {
	return source$.pipe(
	  map(mappingFunction),
	  distinctUntilChanged(memoizationFunction || defaultMemoization),
	  shareReplay(1)
	)
}



export class UniqueBehaviorSubject<T> extends BehaviorSubject<T> {
    constructor(initialData: T) {
        super(deepFreeze(initialData));
    }

    next(newData: T): void {
        const frozenData = deepFreeze(newData);
        if (!naiveObjectComparison(frozenData, this.getValue())) {
            super.next(frozenData);
        }
    }

    update(data: Partial<T>) {
		this.next({ ...this.getValue(), ...data });
	}
}