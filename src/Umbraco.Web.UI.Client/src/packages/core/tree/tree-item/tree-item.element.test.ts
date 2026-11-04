import { UmbTreeItemElement } from './tree-item.element.js';
import { aTimeout, expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

describe('UmbTreeItemElement', () => {
	const registrySubscriberCount = () =>
		(umbExtensionsRegistry as unknown as { _extensions: { _subject: { observers: Array<unknown> } } })._extensions
			._subject.observers.length;

	it('is defined with its own instance', () => {
		expect(document.createElement('umb-tree-item')).to.be.instanceOf(UmbTreeItemElement);
	});

	it('holds no subscriptions on the extension registry once removed', async () => {
		const before = registrySubscriberCount();

		for (let i = 0; i < 10; i++) {
			const element = document.createElement('umb-tree-item');
			element.entityType = 'test-entity-type';
			document.body.appendChild(element);
			await aTimeout(0);
			element.remove();
		}
		await aTimeout(0);

		expect(registrySubscriberCount()).to.equal(before);
	});
});
