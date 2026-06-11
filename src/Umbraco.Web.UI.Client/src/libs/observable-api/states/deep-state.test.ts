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

	describe('getMutePromise', () => {
		it('returns false immediately when state is not muted', async () => {
			const result = await subject.getMutePromise();
			expect(result).to.be.false;
		});

		it('resolves with true when unmute is called', async () => {
			subject.mute();
			const promise = subject.getMutePromise();
			subject.unmute();
			const result = await promise;
			expect(result).to.be.true;
		});

		it('resolves with true when unmute is called even if no emission occurs', async () => {
			subject.mute();
			const promise = subject.getMutePromise();
			// setValue to the same content — unmute will not emit, but the promise must still resolve
			subject.setValue({ key: 'some', another: 'myValue' });
			subject.unmute();
			const result = await promise;
			expect(result).to.be.true;
		});

		it('resolves all concurrent callers on unmute', async () => {
			subject.mute();
			const promises = [subject.getMutePromise(), subject.getMutePromise(), subject.getMutePromise()];
			subject.unmute();
			const results = await Promise.all(promises);
			expect(results).to.deep.equal([true, true, true]);
		});

		it('resolves with false when state is destroyed while muted', async () => {
			subject.mute();
			const promise = subject.getMutePromise();
			subject.destroy();
			const result = await promise;
			expect(result).to.be.false;
		});
	});

	describe('destroy', () => {
		it('throws when setValue is called after destroy', () => {
			subject.destroy();
			expect(() => subject.setValue({ key: 'some', another: 'value' })).to.throw();
		});

		it('throws when unmute is called after destroy', () => {
			subject.mute();
			subject.destroy();
			expect(() => subject.unmute()).to.throw();
		});
	});
});
