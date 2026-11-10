import { expect } from '@open-wc/testing';
import { umbElementPermissionVerbsSatisfy, umbResolveElementPermissionVerbs } from './element-permission-verbs.function.js';

const elementPermission = (id: string, verbs: Array<string>) => ({
	$type: 'ElementPermissionPresentationModel',
	element: { id },
	verbs,
});

describe('umbResolveElementPermissionVerbs', () => {
	it('uses the fallback verbs when no granular permission is on the path', () => {
		const verbs = umbResolveElementPermissionVerbs({
			permissions: [elementPermission('other', ['B'])],
			fallbackPermissions: ['A'],
			path: ['root-folder', 'folder'],
		});

		expect(verbs).to.deep.equal(['A']);
	});

	it('uses the closest granular permission on the path', () => {
		const verbs = umbResolveElementPermissionVerbs({
			permissions: [elementPermission('root-folder', ['B']), elementPermission('folder', ['C'])],
			fallbackPermissions: ['A'],
			path: ['root-folder', 'folder'],
		});

		expect(verbs).to.deep.equal(['C']);
	});

	it('inherits a granular permission from an ancestor', () => {
		const verbs = umbResolveElementPermissionVerbs({
			permissions: [elementPermission('root-folder', ['B'])],
			fallbackPermissions: ['A'],
			path: ['root-folder', 'folder'],
		});

		expect(verbs).to.deep.equal(['B']);
	});

	it('ignores permissions that are not element permissions', () => {
		const verbs = umbResolveElementPermissionVerbs({
			permissions: [{ $type: 'DocumentPermissionPresentationModel', document: { id: 'folder' }, verbs: ['B'] }],
			fallbackPermissions: ['A'],
			path: ['folder'],
		});

		expect(verbs).to.deep.equal(['A']);
	});

	it('uses the fallback verbs for the root', () => {
		const verbs = umbResolveElementPermissionVerbs({
			permissions: [elementPermission('folder', ['B'])],
			fallbackPermissions: ['A'],
			path: [],
		});

		expect(verbs).to.deep.equal(['A']);
	});
});

describe('umbElementPermissionVerbsSatisfy', () => {
	it('requires all of the allOf verbs', () => {
		expect(umbElementPermissionVerbsSatisfy(['A', 'B'], { allOf: ['A', 'B'] })).to.be.true;
		expect(umbElementPermissionVerbsSatisfy(['A'], { allOf: ['A', 'B'] })).to.be.false;
	});

	it('requires at least one of the oneOf verbs', () => {
		expect(umbElementPermissionVerbsSatisfy(['B'], { oneOf: ['A', 'B'] })).to.be.true;
		expect(umbElementPermissionVerbsSatisfy(['C'], { oneOf: ['A', 'B'] })).to.be.false;
	});

	it('is not satisfied without requirements', () => {
		expect(umbElementPermissionVerbsSatisfy(['A'], {})).to.be.false;
	});
});
