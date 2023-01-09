import { BehaviorSubject } from "rxjs";


function deepFreeze<T>(inObj: T): T {
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
    return inObj;
}


export function naiveObjectComparison(objOne: any, objTwo: any): boolean {
    return JSON.stringify(objOne) === JSON.stringify(objTwo);
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