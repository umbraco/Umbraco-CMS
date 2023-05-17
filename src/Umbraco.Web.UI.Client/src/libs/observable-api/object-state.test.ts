import { expect } from '@open-wc/testing';
import { UmbObjectState } from './object-state';

describe('UmbObjectState', () => {
	type ObjectType = { key: string; another: string };

	let subject: UmbObjectState<ObjectType>;
	let initialData: ObjectType;

	beforeEach(() => {
		initialData = { key: 'some', another: 'myValue' };
		subject = new UmbObjectState(initialData);
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

	it('use getObservablePart, updates on its specific change.', (done) => {
		let amountOfCallbacks = 0;

		const subObserver = subject.getObservablePart((data) => data.another);
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

		subject.update({ key: 'change_this_first_should_not_trigger_update' });
		subject.update({ another: 'myNewValue' });
	});
});
