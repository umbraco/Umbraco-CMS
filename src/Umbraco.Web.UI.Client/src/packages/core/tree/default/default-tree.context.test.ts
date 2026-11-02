import { UmbDefaultTreeContext } from './default-tree.context.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbTreeItemModel, UmbTreeRootModel, UmbTreeStartNode } from '../types.js';

@customElement('umb-test-default-tree-context-host')
class UmbTestDefaultTreeContextHostElement extends UmbElementMixin(HTMLElement) {}

const first: UmbTreeStartNode = { unique: 'first', entityType: 'test-entity-type' };
const second: UmbTreeStartNode = { unique: 'second', entityType: 'test-entity-type' };

describe('UmbDefaultTreeContext', () => {
	let context: UmbDefaultTreeContext<UmbTreeItemModel, UmbTreeRootModel>;

	beforeEach(async () => {
		const host = await fixture<UmbTestDefaultTreeContextHostElement>(
			html`<umb-test-default-tree-context-host></umb-test-default-tree-context-host>`,
		);
		context = new UmbDefaultTreeContext(host);
	});

	describe('start node configuration', () => {
		it('keeps the start node when the start nodes are cleared', () => {
			context.setStartNodes([first, second]);
			context.setStartNode(first);
			context.setStartNodes(undefined);

			expect(context.getStartNode()).to.deep.equal(first);
		});

		it('keeps the start node when start nodes are set', () => {
			context.setStartNode(first);
			context.setStartNodes([first, second]);

			expect(context.getStartNode()).to.deep.equal(first);
		});

		it('keeps the start nodes when the start node is set', () => {
			context.setStartNodes([first, second]);
			context.setStartNode(first);

			expect(context.getStartNodes()).to.deep.equal([first, second]);
		});

		it('clears the start nodes when they are set to undefined', () => {
			context.setStartNodes([first, second]);
			context.setStartNodes(undefined);

			expect(context.getStartNodes()).to.have.lengthOf(0);
		});
	});
});
