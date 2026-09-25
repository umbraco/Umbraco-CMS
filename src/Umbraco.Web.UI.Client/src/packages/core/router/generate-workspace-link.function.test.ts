import { umbGenerateWorkspaceLink } from './generate-workspace-link.function.js';
import { UmbPathPattern } from './path-pattern.class.js';
import { expect } from '@open-wc/testing';

const PATTERN = new UmbPathPattern<{ unique: string }>('edit/:unique', '/section/workspace/entity');

describe('umbGenerateWorkspaceLink', () => {
	describe('with a route path', () => {
		it('links relative to the route path', () => {
			const link = umbGenerateWorkspaceLink({
				pattern: PATTERN,
				params: { unique: '123' },
				routePath: '/section/workspace/entity/ref/',
			});
			expect(link.href).to.eq('/section/workspace/entity/ref/edit/123');
		});

		it('navigates in place', () => {
			const link = umbGenerateWorkspaceLink({
				pattern: PATTERN,
				params: { unique: '123' },
				routePath: '/section/workspace/entity/ref/',
			});
			expect(link.target).to.be.undefined;
		});

		it('joins with a single slash whether or not the route path ends with one', () => {
			const withSlash = umbGenerateWorkspaceLink({
				pattern: PATTERN,
				params: { unique: '123' },
				routePath: '/section/workspace/entity/ref/',
			});
			const withoutSlash = umbGenerateWorkspaceLink({
				pattern: PATTERN,
				params: { unique: '123' },
				routePath: '/section/workspace/entity/ref',
			});
			expect(withSlash.href).to.eq(withoutSlash.href);
			expect(withSlash.href).to.not.contain('//');
		});
	});

	describe('without a route path', () => {
		it('links to the absolute workspace path', () => {
			const link = umbGenerateWorkspaceLink({ pattern: PATTERN, params: { unique: '123' } });
			expect(link.href).to.eq('/section/workspace/entity/edit/123');
		});

		it('opens in a new tab', () => {
			const link = umbGenerateWorkspaceLink({ pattern: PATTERN, params: { unique: '123' } });
			expect(link.target).to.eq('_blank');
		});

		it('treats an empty route path as no route path', () => {
			const link = umbGenerateWorkspaceLink({ pattern: PATTERN, params: { unique: '123' }, routePath: '' });
			expect(link.href).to.eq('/section/workspace/entity/edit/123');
			expect(link.target).to.eq('_blank');
		});
	});

	it('supports patterns with more than one parameter', () => {
		const pattern = new UmbPathPattern<{ parentUnique: string; unique: string }>(
			'create/:parentUnique/:unique',
			'/section/workspace/entity',
		);
		const link = umbGenerateWorkspaceLink({ pattern, params: { parentUnique: 'abc', unique: '123' } });
		expect(link.href).to.eq('/section/workspace/entity/create/abc/123');
	});
});
