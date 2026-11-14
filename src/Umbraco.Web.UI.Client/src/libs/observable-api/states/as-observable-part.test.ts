import { umbIsStatefulObservable } from '../utils/stateful-observable.function.js';
import type { UmbBasicState } from './basic-state.js';
import { UmbArrayState } from './array-state.js';
import { UmbClassState } from './class-state.js';
import { UmbObjectState } from './object-state.js';
import { UmbStringState } from './string-state.js';
import { expect } from '@open-wc/testing';
import type { BehaviorSubject, Observable } from '@umbraco-cms/backoffice/external/rxjs';

class UmbTestClassStateData {
	constructor(public value: string) {}
	equal(other: UmbTestClassStateData | undefined) {
		return other?.value === this.value;
	}
}

const subjectOf = (state: UmbBasicState<unknown>) =>
	(state as unknown as { _subject: BehaviorSubject<unknown> })._subject;

const states: Array<{ name: string; create: () => UmbBasicState<any> }> = [
	{ name: 'UmbStringState', create: () => new UmbStringState('value') },
	{ name: 'UmbArrayState', create: () => new UmbArrayState([{ unique: '1' }], (x) => x.unique) },
	{ name: 'UmbObjectState', create: () => new UmbObjectState({ value: 'value' }) },
	{ name: 'UmbClassState', create: () => new UmbClassState(new UmbTestClassStateData('value')) },
];

describe('Observables of States', () => {
	for (const { name, create } of states) {
		describe(name, () => {
			it('releases the state when the last subscriber of a part unsubscribes', () => {
				const state = create();
				const part = state.asObservablePart((x) => x);

				part.subscribe().unsubscribe();

				expect(subjectOf(state).observed).to.be.false;
			});

			describe('when the state is destroyed', () => {
				const subscribeLate = (part: Observable<unknown>) => {
					const received: Array<string> = [];
					part.subscribe({ next: () => received.push('value'), complete: () => received.push('complete') });
					return received;
				};

				it('completes a released part without a value', () => {
					const state = create();
					const part = state.asObservablePart((x) => x);
					part.subscribe().unsubscribe();

					state.destroy();

					expect(subscribeLate(part)).to.deep.equal(['complete']);
				});

				it('gives later subscribers the last value of a part that was still observed', () => {
					const state = create();
					const part = state.asObservablePart((x) => x);
					const subscription = part.subscribe();

					state.destroy();
					subscription.unsubscribe();

					expect(subscribeLate(part)).to.deep.equal(['value', 'complete']);
				});
			});

			it('provides stateful observables', () => {
				const state = create();
				expect(umbIsStatefulObservable(state.asObservable())).to.be.true;
				expect(umbIsStatefulObservable(state.asObservablePart((x) => x))).to.be.true;
			});
		});
	}
});
