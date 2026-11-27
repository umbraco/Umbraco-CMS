import { createObservablePart } from './create-observable-part.function.js';
import { umbIsStatefulObservable, umbMarkAsStatefulObservable } from './stateful-observable.function.js';
import { expect } from '@open-wc/testing';
import { BehaviorSubject, Subject } from '@umbraco-cms/backoffice/external/rxjs';

describe('createObservablePart', () => {
	describe('with a stateful observable as source', () => {
		let source: BehaviorSubject<{ value: string }>;

		beforeEach(() => {
			source = umbMarkAsStatefulObservable(new BehaviorSubject({ value: 'initial' }));
		});

		it('releases the source when the last subscriber unsubscribes', () => {
			const part = createObservablePart(source, (x) => x.value);

			const first = part.subscribe();
			const second = part.subscribe();
			expect(source.observed).to.be.true;

			first.unsubscribe();
			expect(source.observed).to.be.true;

			second.unsubscribe();
			expect(source.observed).to.be.false;
		});

		it('gives a later subscriber the current value of the source', () => {
			const part = createObservablePart(source, (x) => x.value);
			part.subscribe().unsubscribe();

			source.next({ value: 'changed' });

			const received: Array<string> = [];
			part.subscribe((value) => received.push(value)).unsubscribe();
			expect(received).to.deep.equal(['changed']);
		});

		it('does not emit values equal to the previous one', () => {
			const part = createObservablePart(source, (x) => x.value);
			const received: Array<string> = [];
			const subscription = part.subscribe((value) => received.push(value));

			source.next({ value: 'initial' });
			source.next({ value: 'changed' });
			subscription.unsubscribe();

			expect(received).to.deep.equal(['initial', 'changed']);
		});

		it('is itself a stateful observable', () => {
			const part = createObservablePart(source, (x) => x.value);
			expect(umbIsStatefulObservable(part)).to.be.true;

			createObservablePart(part, (x) => x.length)
				.subscribe()
				.unsubscribe();
			expect(source.observed).to.be.false;
		});
	});

	describe('with a source that is not a stateful observable', () => {
		it('keeps the source subscribed after the last subscriber unsubscribes', () => {
			const source = new Subject<{ value: string }>();
			const part = createObservablePart(source, (x) => x.value);

			part.subscribe().unsubscribe();

			expect(source.observed).to.be.true;
		});

		it('gives a later subscriber the last value from the source', () => {
			const source = new Subject<{ value: string }>();
			const part = createObservablePart(source, (x) => x.value);
			part.subscribe().unsubscribe();

			source.next({ value: 'emitted once' });

			const received: Array<string> = [];
			part.subscribe((value) => received.push(value)).unsubscribe();
			expect(received).to.deep.equal(['emitted once']);
		});

		it('is not a stateful observable', () => {
			const part = createObservablePart(new Subject<{ value: string }>(), (x) => x.value);
			expect(umbIsStatefulObservable(part)).to.be.false;
		});
	});
});
