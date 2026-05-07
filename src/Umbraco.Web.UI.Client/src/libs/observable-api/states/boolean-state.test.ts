import { UmbBooleanState } from './boolean-state.js';
import { expect } from '@open-wc/testing';

describe('UmbBooleanState', () => {
	it('initial value is reflected on subscription', (done) => {
		const state = new UmbBooleanState(false);
		state.asObservable().subscribe((value) => {
			expect(value).to.be.false;
			done();
		});
	});

	it('getValue returns the current value', () => {
		const state = new UmbBooleanState(true);
		expect(state.getValue()).to.be.true;
		state.setValue(false);
		expect(state.getValue()).to.be.false;
	});

	it('does not fire a change when going from false to false', () => {
		const state = new UmbBooleanState(false);
		let amountOfCallbacks = 0;

		state.asObservable().subscribe(() => {
			amountOfCallbacks++;
		});

		// Initial subscription emits once.
		expect(amountOfCallbacks).to.equal(1);

		state.setValue(false);
		state.setValue(false);
		state.setValue(false);

		expect(amountOfCallbacks).to.equal(1);
	});

	it('does not fire a change when going from true to true', () => {
		const state = new UmbBooleanState(true);
		let amountOfCallbacks = 0;

		state.asObservable().subscribe(() => {
			amountOfCallbacks++;
		});

		expect(amountOfCallbacks).to.equal(1);

		state.setValue(true);
		state.setValue(true);

		expect(amountOfCallbacks).to.equal(1);
	});

	it('fires a change when going from false to true', (done) => {
		const state = new UmbBooleanState(false);
		let amountOfCallbacks = 0;

		state.asObservable().subscribe((value) => {
			amountOfCallbacks++;
			if (amountOfCallbacks === 1) {
				expect(value).to.be.false;
			}
			if (amountOfCallbacks === 2) {
				expect(value).to.be.true;
				done();
			}
		});

		state.setValue(true);
	});

	it('fires a change when going from true to false', (done) => {
		const state = new UmbBooleanState(true);
		let amountOfCallbacks = 0;

		state.asObservable().subscribe((value) => {
			amountOfCallbacks++;
			if (amountOfCallbacks === 1) {
				expect(value).to.be.true;
			}
			if (amountOfCallbacks === 2) {
				expect(value).to.be.false;
				done();
			}
		});

		state.setValue(false);
	});

	it('only fires once when toggled then set back to the same value in a row', () => {
		const state = new UmbBooleanState(false);
		const emitted: Array<boolean> = [];

		state.asObservable().subscribe((value) => {
			emitted.push(value as boolean);
		});

		state.setValue(true);
		state.setValue(true);
		state.setValue(false);
		state.setValue(false);

		expect(emitted).to.deep.equal([false, true, false]);
	});

	it('replays the latest value to late subscribers', (done) => {
		const state = new UmbBooleanState(false);
		state.setValue(true);

		state.asObservable().subscribe((value) => {
			expect(value).to.be.true;
			done();
		});
	});

	it('does not emit after destroy', () => {
		const state = new UmbBooleanState(false);
		let amountOfCallbacks = 0;

		state.asObservable().subscribe(() => {
			amountOfCallbacks++;
		});

		expect(amountOfCallbacks).to.equal(1);

		state.destroy();

		// Setting after destroy must not throw and must not emit.
		expect(() => state.setValue(true)).to.not.throw();
		expect(amountOfCallbacks).to.equal(1);
	});
});
