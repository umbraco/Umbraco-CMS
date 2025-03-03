import { retrieveStoredPath, setStoredPath, UMB_STORAGE_REDIRECT_URL } from './stored-path.function.js';
import { expect } from '@open-wc/testing';

describe('retrieveStoredPath', () => {
	beforeEach(() => {
		sessionStorage.clear();
	});

	it('should return a null if no path is stored', () => {
		expect(retrieveStoredPath()).to.be.null;
	});

	it('should return the stored path if a path is stored', () => {
		const testSafeUrl = new URL('/test', window.location.origin);
		setStoredPath(testSafeUrl.toString());
		expect(retrieveStoredPath()?.toString()).to.eq(testSafeUrl.toString());
	});

	it('should remove the stored path after it is retrieved', () => {
		setStoredPath('/test');
		retrieveStoredPath();
		expect(sessionStorage.getItem(UMB_STORAGE_REDIRECT_URL)).to.be.null;
	});

	it('should return null if the stored path ends with "logout"', () => {
		setStoredPath('/logout');
		expect(retrieveStoredPath()).to.be.null;
	});

	it('should not be possible to trick it with a fake URL', () => {
		setStoredPath('//www.google.com');
		expect(retrieveStoredPath()).to.be.null;

		// also test setting it directly in sessionStorage (this will return the current path instead of the fake path)
		sessionStorage.setItem(UMB_STORAGE_REDIRECT_URL, '//www.google.com');
		expect(retrieveStoredPath()?.pathname).to.eq(window.location.pathname);
	});
});

describe('setStoredPath', () => {
	beforeEach(() => {
		sessionStorage.clear();
	});

	it('should store a local path', () => {
		const testSafeUrl = new URL('/test', window.location.origin);
		setStoredPath(testSafeUrl.toString());
		expect(sessionStorage.getItem(UMB_STORAGE_REDIRECT_URL)).to.eq(testSafeUrl.toString());
	});

	it('should not store a non-local path', () => {
		setStoredPath('https://example.com/test');
		expect(sessionStorage.getItem(UMB_STORAGE_REDIRECT_URL)).to.be.null;
	});
});
