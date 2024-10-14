import { UmbObjectState } from './object-state.js';
import { expect } from '@open-wc/testing';

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

		subject.update({ key: 'change_this_first_should_not_trigger_update' });
		subject.update({ another: 'myNewValue' });
	});

	it('replays the latests value when unmuted.', (done) => {
		let amountOfCallbacks = 0;

		const observer = subject.asObservable();
		observer.subscribe((value) => {
			amountOfCallbacks++;
			if (amountOfCallbacks === 1) {
				// First callback gives us the initialized value.
				expect(value.key).to.be.equal('some');
			}
			if (amountOfCallbacks === 2) {
				// Second callback gives us the first change.
				expect(value.key).to.be.equal('firstChange');
			}
			if (amountOfCallbacks === 3) {
				// Third callback gives us the last change before unmuted.
				expect(value.key).to.be.equal('thirdChange');
				done();
			}
		});

		subject.update({ key: 'firstChange' });
		subject.mute();
		subject.update({ key: 'secondChange' });
		subject.update({ key: 'thirdChange' });
		subject.unmute();
	});

	/*
	it('replays latests unmuted value when muted.', (done) => {
		const observer = subject.asObservable();
		observer.subscribe((value) => {
			expect(value).to.be.equal(initialData);
		});

		subject.mute();
		subject.update({ key: 'firstChange' });

		observer.subscribe((value) => {
			expect(value).to.be.equal(initialData);
			done();
		});

	});
	*/
});
