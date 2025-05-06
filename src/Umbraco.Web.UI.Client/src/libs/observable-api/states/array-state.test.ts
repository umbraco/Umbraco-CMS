import { UmbArrayState } from './array-state.js';
import { expect } from '@open-wc/testing';

describe('ArrayState', () => {
	type ObjectType = { key: string; another: string };
	type ArrayType = ObjectType[];

	let subject: UmbArrayState<ObjectType>;
	let initialData: ArrayType;

	beforeEach(() => {
		initialData = [
			{ key: '1', another: 'myValue1' },
			{ key: '2', another: 'myValue2' },
			{ key: '3', another: 'myValue3' },
		];
		subject = new UmbArrayState(initialData, (x) => x.key);
	});

	it('replays latests, no matter the amount of subscriptions.', (done) => {
		let amountOfCallbacks = 0;
		const observer = subject.asObservable();
		observer.subscribe((value) => {
			amountOfCallbacks++;
			expect(value).to.be.equal(initialData);
		});
		observer.subscribe((value) => {
			amountOfCallbacks++;
			expect(value).to.be.equal(initialData);
			if (amountOfCallbacks === 2) {
				done();
			}
		});
	});

	it('remove method, removes the one with the key', (done) => {
		const expectedData = [initialData[0], initialData[2]];

		subject.remove(['2']);
		const observer = subject.asObservable();
		observer.subscribe((value) => {
			expect(JSON.stringify(value)).to.be.equal(JSON.stringify(expectedData));
			done();
		});
	});

	it('getHasOne method, return true when key exists', () => {
		expect(subject.getHasOne('2')).to.be.true;
	});
	it('getHasOne method, return false when key does not exists', () => {
		expect(subject.getHasOne('1337')).to.be.false;
	});

	it('filter method, removes anything that is not true of the given predicate method', (done) => {
		const expectedData = [initialData[0], initialData[2]];

		subject.filter((x) => x.key !== '2');
		const observer = subject.asObservable();
		observer.subscribe((value) => {
			expect(JSON.stringify(value)).to.be.equal(JSON.stringify(expectedData));
			done();
		});
	});

	it('add new item via appendOne method.', (done) => {
		const newItem = { key: '4', another: 'myValue4' };
		subject.appendOne(newItem);

		const expectedData = [...initialData, newItem];

		const observer = subject.asObservable();
		observer.subscribe((value) => {
			expect(value.length).to.be.equal(expectedData.length);
			expect(value[3].another).to.be.equal(expectedData[3].another);
			done();
		});
	});

	it('partially update an existing item via updateOne method.', (done) => {
		const newItem = { another: 'myValue2.2' };
		subject.updateOne('2', newItem);

		const observer = subject.asObservable();
		observer.subscribe((value) => {
			expect(value.length).to.be.equal(initialData.length);
			expect(value[0].another).to.be.equal('myValue1');
			expect(value[1].another).to.be.equal('myValue2.2');
			done();
		});
	});

	it('getObservablePart for a specific entry of array', (done) => {
		const subObserver = subject.asObservablePart((data) => data.find((x) => x.key === '2'));
		subObserver.subscribe((entry) => {
			if (entry) {
				expect(entry.another).to.be.equal(initialData[1].another);
				done();
			}
		});
	});

	it('getObservablePart returns undefined if item does not exist', (done) => {
		let amountOfCallbacks = 0;
		const newItem = { key: '4', another: 'myValue4' };

		const subObserver = subject.asObservablePart((data) => data.find((x) => x.key === newItem.key));
		subObserver.subscribe((entry) => {
			amountOfCallbacks++;
			if (amountOfCallbacks === 1) {
				expect(entry).to.be.equal(undefined); // First callback should give null, cause we didn't have this entry when the subscription was made.
			}
			if (amountOfCallbacks === 2) {
				expect(entry).to.be.equal(newItem); // Second callback should give us the right data:
				if (entry) {
					expect(entry.another).to.be.equal(newItem.another);
					done();
				}
			}
		});

		subject.appendOne(newItem);
	});

	it('asObservable returns the replaced item', (done) => {
		const newItem = { key: '2', another: 'myValue4' };
		subject.appendOne(newItem);

		const expectedData = [initialData[0], newItem, initialData[2]];

		const observer = subject.asObservable();
		observer.subscribe((value) => {
			expect(value.length).to.be.equal(expectedData.length);
			expect(value[1].another).to.be.equal(newItem.another);
			done();
		});
	});

	it('getObservablePart returns the replaced item', (done) => {
		const newItem = { key: '2', another: 'myValue4' };
		subject.appendOne(newItem);

		const subObserver = subject.asObservablePart((data) => data.find((x) => x.key === newItem.key));
		subObserver.subscribe((entry) => {
			expect(entry).to.be.equal(newItem); // Second callback should give us the right data:
			if (entry) {
				expect(entry.another).to.be.equal(newItem.another);
				done();
			}
		});
	});

	it('getObservablePart replays existing data to any amount of subscribers.', (done) => {
		let amountOfCallbacks = 0;

		const subObserver = subject.asObservablePart((data) => data.find((x) => x.key === '2'));
		subObserver.subscribe((entry) => {
			if (entry) {
				amountOfCallbacks++;
				expect(entry.another).to.be.equal(initialData[1].another);
			}
		});
		subObserver.subscribe((entry) => {
			if (entry) {
				amountOfCallbacks++;
				expect(entry.another).to.be.equal(initialData[1].another);
				if (amountOfCallbacks === 2) {
					done();
				}
			}
		});
	});

	it('getObservablePart replays existing data to any amount of subscribers.', (done) => {
		let amountOfCallbacks = 0;

		const subObserver = subject.asObservablePart((data) => data.find((x) => x.key === '2'));
		subObserver.subscribe((entry) => {
			if (entry) {
				amountOfCallbacks++;
				expect(entry.another).to.be.equal(initialData[1].another);
			}
		});
		subObserver.subscribe((entry) => {
			if (entry) {
				amountOfCallbacks++;
				expect(entry.another).to.be.equal(initialData[1].another);
				if (amountOfCallbacks === 2) {
					done();
				}
			}
		});
	});

	it('append only updates observable if changes item', (done) => {
		let count = 0;

		const observer = subject.asObservable();
		observer.subscribe((value) => {
			count++;
			if (count === 1) {
				expect(value.length).to.be.equal(initialData.length);
				expect(value[0]).to.be.equal(initialData[0]);
				expect(value[0].another).to.be.equal(initialData[0].another);
				expect(value[1].another).to.be.equal(initialData[1].another);
				expect(value[2].another).to.be.equal(initialData[2].another);
			} else if (count === 2) {
				expect(value.length).to.be.equal(4);
				expect(value[3].another).to.be.equal('myValue4');
				done();
			}
		});

		Promise.resolve().then(() => {
			// Despite how many times this happens it should not trigger any change.
			subject.append(initialData);
			subject.append(initialData);
			subject.append(initialData);

			Promise.resolve().then(() => {
				subject.appendOne({ key: '4', another: 'myValue4' });
			});
		});
	});
});
