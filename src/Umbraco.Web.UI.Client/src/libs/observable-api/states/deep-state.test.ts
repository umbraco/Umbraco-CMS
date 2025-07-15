import { UmbDeepState } from './deep-state.js';
import { expect } from '@open-wc/testing';

describe('UmbDeepState', () => {
	type ObjectType = { key: string; another: string };

	let subject: UmbDeepState<ObjectType>;
	let initialData: ObjectType;

	beforeEach(() => {
		initialData = { key: 'some', another: 'myValue' };
		subject = new UmbDeepState(initialData);
	});

	it('getValue gives the initial data', () => {
		expect(subject.getValue().another).to.be.equal(initialData.another);
	});

	it('update via next', () => {
		subject.setValue({ key: 'some', another: 'myNewValue' });
		expect(subject.getValue().another).to.be.equal('myNewValue');
	});

	it('replays latests, no matter the amount of subscriptions.', (done) => {
		const observer = subject.asObservable();
		observer.subscribe((value) => {
			expect(value).to.be.equal(initialData);
		});
		observer.subscribe((value) => {
			expect(value).to.be.equal(initialData);
			done();
		});
	});

	it('use gObservablePart, updates on its specific change.', (done) => {
		let amountOfCallbacks = 0;

		const subObserver = subject.asObservablePart((data) => data.another);
		subObserver.subscribe((value) => {
			amountOfCallbacks++;
			if (amountOfCallbacks === 1) {
				expect(value).to.be.equal('myValue');
			}
			if (amountOfCallbacks === 2) {
				expect(value).to.be.equal('myNewValue');
				done();
			}
		});

		subject.setValue({ key: 'change_this_first_should_not_trigger_update', another: 'myValue' });
		subject.setValue({ key: 'some', another: 'myNewValue' });
	});
});
