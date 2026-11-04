import { mergeObservables } from './merge-observables.function.js';
import { umbIsStatefulObservable, umbMarkAsStatefulObservable } from './stateful-observable.function.js';
import { expect } from '@open-wc/testing';
import { BehaviorSubject } from '@umbraco-cms/backoffice/external/rxjs';

describe('mergeObservables', () => {
	describe('with only stateful observables as sources', () => {
		let first: BehaviorSubject<number>;
		let second: BehaviorSubject<number>;

		beforeEach(() => {
			first = umbMarkAsStatefulObservable(new BehaviorSubject(1));
			second = umbMarkAsStatefulObservable(new BehaviorSubject(2));
		});

		it('releases all sources when the last subscriber unsubscribes', () => {
			const merged = mergeObservables([first, second], ([a, b]) => a + b);

			merged.subscribe().unsubscribe();

			expect(first.observed).to.be.false;
			expect(second.observed).to.be.false;
		});

		it('gives a later subscriber the current merged value', () => {
			const merged = mergeObservables([first, second], ([a, b]) => a + b);
			merged.subscribe().unsubscribe();

			first.next(10);

			const received: Array<number> = [];
			merged.subscribe((value) => received.push(value)).unsubscribe();
			expect(received).to.deep.equal([12]);
		});

		it('is itself a stateful observable', () => {
			expect(umbIsStatefulObservable(mergeObservables([first, second], ([a, b]) => a + b))).to.be.true;
		});
	});

	describe('with a source that is not a stateful observable', () => {
		it('keeps its sources subscribed after the last subscriber unsubscribes', () => {
			const statefulObservable = umbMarkAsStatefulObservable(new BehaviorSubject(1));
			const notMarked = new BehaviorSubject(2);
			const merged = mergeObservables([statefulObservable, notMarked], ([a, b]) => a + b);

			merged.subscribe().unsubscribe();

			expect(statefulObservable.observed).to.be.true;
			expect(notMarked.observed).to.be.true;
			expect(umbIsStatefulObservable(merged)).to.be.false;
		});
	});
});
