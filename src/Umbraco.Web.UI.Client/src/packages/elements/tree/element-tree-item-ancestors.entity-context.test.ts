import { UmbElementTreeItemAncestorsEntityContext } from './element-tree-item-ancestors.entity-context.js';
import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../entity.js';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbAncestorsEntityContext } from '@umbraco-cms/backoffice/entity';

@customElement('test-tree-item-ancestors-parent-host')
class UmbTestTreeItemAncestorsParentHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	ancestorsContext = new UmbAncestorsEntityContext(this);
}

@customElement('test-tree-item-ancestors-child-host')
class UmbTestTreeItemAncestorsChildHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	ancestorsContext = new UmbElementTreeItemAncestorsEntityContext(this);
}

describe('UmbElementTreeItemAncestorsEntityContext', () => {
	let parentHost: UmbTestTreeItemAncestorsParentHostElement;
	let childHost: UmbTestTreeItemAncestorsChildHostElement;

	const folder = (unique: string) => ({ unique, entityType: UMB_ELEMENT_FOLDER_ENTITY_TYPE });

	beforeEach(() => {
		parentHost = new UmbTestTreeItemAncestorsParentHostElement();
		childHost = new UmbTestTreeItemAncestorsChildHostElement();
		parentHost.appendChild(childHost);
		document.body.appendChild(parentHost);
	});

	afterEach(() => {
		parentHost.remove();
	});

	it('provides the parent appended to the ancestors of the parent tree item', async () => {
		parentHost.ancestorsContext.setAncestors([folder('a')]);
		childHost.ancestorsContext.setParentUnique('b');
		await aTimeout(0);

		expect(childHost.ancestorsContext.getAncestors().map((x) => x.unique)).to.deep.equal(['a', 'b']);
	});

	it('provides only the parent when the parent tree item has no ancestors', async () => {
		childHost.ancestorsContext.setParentUnique('a');
		await aTimeout(0);

		expect(childHost.ancestorsContext.getAncestors().map((x) => x.unique)).to.deep.equal(['a']);
	});

	it('provides no ancestors for an item without a parent, ignoring surrounding ancestors', async () => {
		parentHost.ancestorsContext.setAncestors([folder('a')]);
		childHost.ancestorsContext.setParentUnique(null);
		await aTimeout(0);

		expect(childHost.ancestorsContext.getAncestors()).to.deep.equal([]);
	});
});
