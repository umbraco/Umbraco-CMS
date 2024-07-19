import { hasOwnOpener } from './has-own-opener.function.js';
import { expect } from '@open-wc/testing';

describe('hasOwnOpener', () => {
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	let mockWindow: any;

	beforeEach(() => {
		mockWindow = {
			location: {
				origin: 'http://localhost',
				pathname: '/test',
			},
		};
	});

	it('should return false if there is no opener', () => {
		expect(hasOwnOpener(undefined, mockWindow)).to.be.false;
	});

	it('should return false if the opener is from a different origin', () => {
		mockWindow.opener = {
			location: {
				origin: 'https://example.com',
				pathname: '/test',
			},
		};

		expect(hasOwnOpener(undefined, mockWindow)).to.be.false;
	});

	it('should return true if the opener is from the same origin and no pathname is specified', () => {
		mockWindow.opener = {
			location: {
				origin: 'http://localhost',
				pathname: '/test',
			},
		};

		expect(hasOwnOpener(undefined, mockWindow)).to.be.true;
	});

	it('should return false if the opener is from the same origin but has a different pathname', () => {
		mockWindow.opener = {
			location: {
				origin: 'http://localhost',
				pathname: '/different',
			},
		};

		expect(hasOwnOpener('/test', mockWindow)).to.be.false;
	});

	it('should return true if the opener is from the same origin and has the same pathname', () => {
		mockWindow.opener = {
			location: {
				origin: 'http://localhost',
				pathname: '/test',
			},
		};

		expect(hasOwnOpener('/test', mockWindow)).to.be.true;
	});
});
