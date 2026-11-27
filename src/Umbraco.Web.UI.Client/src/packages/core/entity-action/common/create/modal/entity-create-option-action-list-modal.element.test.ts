import type { UmbEntityCreateOptionActionListModalData } from './entity-create-option-action-list-modal.token.js';
import { UmbEntityCreateOptionActionListModalElement } from './entity-create-option-action-list-modal.element.js';
import { expect, waitUntil } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbEntityCreateOptionActionBase } from '@umbraco-cms/backoffice/entity-create-option-action';
import type { UmbRefItemElement } from '@umbraco-cms/backoffice/components';

const ENTITY_TYPE = 'test-entity';
const LINK_OPTION_ALIAS = 'Umb.Test.EntityCreateOptionActionListModal.Link';
const HREF = 'section/test/workspace/test-entity/create';

let releaseHref: () => void;
let hrefReady: Promise<void>;
let apiCreated: boolean;

class UmbTestLinkCreateOptionAction extends UmbEntityCreateOptionActionBase {
	constructor(...args: ConstructorParameters<typeof UmbEntityCreateOptionActionBase>) {
		super(...args);
		apiCreated = true;
	}

	override async getHref() {
		await hrefReady;
		return HREF;
	}
}

describe('UmbEntityCreateOptionActionListModalElement', () => {
	let element: UmbEntityCreateOptionActionListModalElement;

	beforeEach(() => {
		apiCreated = false;
		hrefReady = new Promise((resolve) => (releaseHref = resolve));

		umbExtensionsRegistry.register({
			type: 'entityCreateOptionAction',
			alias: LINK_OPTION_ALIAS,
			name: 'Test Link Create Option',
			api: UmbTestLinkCreateOptionAction,
			forEntityTypes: [ENTITY_TYPE],
			meta: { label: 'Link option' },
		} as never);

		element = new UmbEntityCreateOptionActionListModalElement();
		element.data = { entityType: ENTITY_TYPE, unique: null } as UmbEntityCreateOptionActionListModalData;
		document.body.appendChild(element);
	});

	afterEach(() => {
		element.remove();
		umbExtensionsRegistry.unregister(LINK_OPTION_ALIAS);
	});

	function getRefItems() {
		return [...(element.shadowRoot?.querySelectorAll<UmbRefItemElement>('umb-ref-item') ?? [])];
	}

	it('does not render a link option before its href is known', async () => {
		await waitUntil(() => apiCreated, 'the create option api was never created');
		await element.updateComplete;

		expect(getRefItems().filter((item) => !item.href)).to.have.lengthOf(0);
	});

	it('renders a link option with its href once it is known', async () => {
		releaseHref();

		await waitUntil(() => getRefItems().length > 0, 'the create option was never rendered');
		expect(getRefItems()[0].href).to.equal(HREF);
	});
});
