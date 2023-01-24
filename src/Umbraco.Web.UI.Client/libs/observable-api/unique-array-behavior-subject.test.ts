import { expect } from '@open-wc/testing';
import { UniqueArrayBehaviorSubject } from './unique-array-behavior-subject';
import { createObservablePart } from '@umbraco-cms/observable-api';

describe('UniqueArrayBehaviorSubject', () => {

	type ObjectType = {key: string, another: string};
	type ArrayType = ObjectType[];

	let subject: UniqueArrayBehaviorSubject<ObjectType>;
	let initialData: ArrayType;

	beforeEach(() => {
		initialData = [
			{key: '1', another: 'myValue1'},
			{key: '2', another: 'myValue2'},
			{key: '3', another: 'myValue3'}
		];
		subject = new UniqueArrayBehaviorSubject(initialData, x => x.key);
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

	it('add new item via appendOne method.', (done) => {

		const newItem = {key: '4', another: 'myValue4'};
		subject.appendOne(newItem);

		const expectedData = [...initialData, newItem]

		const observer = subject.asObservable();
		observer.subscribe((value) => {
			expect(value.length).to.be.equal(expectedData.length);
			expect(value[3].another).to.be.equal(expectedData[3].another);
			done();
		});

	});


	it('createObservablePart for a specific entry of array', (done) => {

		const subObserver = createObservablePart(subject, data => data.find(x => x.key === '2'));
		subObserver.subscribe((entry) => {
			if(entry) {
				expect(entry.another).to.be.equal(initialData[1].another);
				done();
			}
		});

	});


	it('createObservablePart returns undefined if item does not exist', (done) => {

		let amountOfCallbacks = 0;
		const newItem = {key: '4', another: 'myValue4'};

		const subObserver = createObservablePart(subject, data => data.find(x => x.key === newItem.key));
		subObserver.subscribe((entry) => {
			amountOfCallbacks++;
			if(amountOfCallbacks === 1) {
				expect(entry).to.be.equal(undefined);// First callback should give null, cause we didn't have this entry when the subscription was made.
			}
			if(amountOfCallbacks === 2) {
				expect(entry).to.be.equal(newItem);// Second callback should give us the right data:
				if(entry) {
					expect(entry.another).to.be.equal(newItem.another);
					done();
				}
			}
		});

		subject.appendOne(newItem);

	});


	it('asObservable returns the replaced item', (done) => {

		const newItem = {key: '2', another: 'myValue4'};
		subject.appendOne(newItem);

		const expectedData = [initialData[0], newItem, initialData[2]];

		const observer = subject.asObservable();
		observer.subscribe((value) => {
			expect(value.length).to.be.equal(expectedData.length);
			expect(value[1].another).to.be.equal(newItem.another);
			done();
		});

	});

	it('createObservablePart returns the replaced item', (done) => {

		const newItem = {key: '2', another: 'myValue4'};
		subject.appendOne(newItem);

		const subObserver = createObservablePart(subject, data => data.find(x => x.key === newItem.key));
		subObserver.subscribe((entry) => {
			expect(entry).to.be.equal(newItem);// Second callback should give us the right data:
			if(entry) {
				expect(entry.another).to.be.equal(newItem.another);
				done();
			}
		});

	});


});
