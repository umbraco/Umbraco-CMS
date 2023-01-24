import { BasicState } from "./basic-state";

/**
 * @export
 * @class NumberState
 * @extends {BehaviorSubject<T>}
 * @description - A RxJS BehaviorSubject this Subject ensures the data is unique, not updating any Observes unless there is an actual change of the value.
 */
export class NumberState<T extends number | undefined | null> extends BasicState<T> {

}
