import { UmbObjectState } from './states/object-state.js';
import { UmbObserver } from './observer.js';
import { expect } from '@open-wc/testing';

describe('UmbObserver', () => {
	type ObjectType = { key: string; another: string };

	let subject: UmbObjectState<ObjectType>;
	let initialData: ObjectType;

	beforeEach(() => {
		initialData = { key: 'some', another: 'myValue' };
		subject = new UmbObjectState(initialData);
	});

	it('gets existing data, no matter the amount of observers.', (done) => {
		const observable = subject.asObservable();
		new UmbObserver(observable, (value) => {
			expect(value).to.be.equal(initialData);
		});
		new UmbObserver(observable, (value) => {
			expect(value).to.be.equal(initialData);
			done();
		});
	});

	it('provides an asPromise, which is trigged ones it has its first value', (done) => {
		let count = 0;
		const lateSubject = new UmbObjectState<ObjectType | undefined>(undefined);
		const observable = lateSubject.asObservable();
		const umbObserver1 = new UmbObserver(observable, (value) => {
			if (count === 0) {
				expect(value?.another).to.be.equal(undefined);
			}
			if (count === 1) {
				expect(value?.another).to.be.equal(initialData.another);
			}
			if (count === 2) {
				expect(value?.another).to.be.equal('myChangedValue');
				done();
			}
			count++;
		});

		// Using promise to first set data, after it has gotten none-undefined data. (promise first resolves ones data is not undefined)
		umbObserver1.asPromise().then(() => {
			lateSubject.update({ another: 'myChangedValue' });
		});

		expect(count).to.be.equal(1);
		lateSubject.setValue(initialData);
	});
});
