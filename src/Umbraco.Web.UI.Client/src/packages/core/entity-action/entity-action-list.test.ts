import { UmbEntityActionListElement } from './entity-action-list.element.js';
import type { ManifestEntityAction } from './entity-action.extension.js';
import { UmbEntityActionBase } from './entity-action-base.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement, html as litHtml } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityContext } from '@umbraco-cms/backoffice/entity';

@customElement('umb-test-entity-action-list-action')
class UmbTestEntityActionElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestEntityActionApi extends UmbEntityActionBase<never> {
	override async execute() {}
}

const ENTITY_TYPE = 'test-entity-action-list';

@customElement('umb-test-entity-action-list-host')
class UmbTestEntityActionListHostElement extends UmbLitElement {
	constructor() {
		super();
		const context = new UmbEntityContext(this);
		context.setEntityType(ENTITY_TYPE);
		context.setUnique('1234');
	}

	override render() {
		return litHtml`<umb-entity-action-list></umb-entity-action-list>`;
	}
}

type TestAction = { alias: string; weight: number; separatorBefore?: boolean };

function sleep(timeMs: number) {
	return new Promise((resolve) => setTimeout(resolve, timeMs));
}

function registerActions(actions: Array<TestAction>) {
	umbExtensionsRegistry.registerMany(
		actions.map(
			(action): ManifestEntityAction => ({
				type: 'entityAction',
				alias: action.alias,
				name: action.alias,
				weight: action.weight,
				separatorBefore: action.separatorBefore,
				forEntityTypes: [ENTITY_TYPE],
				elementName: 'umb-test-entity-action-list-action',
				api: UmbTestEntityActionApi,
				meta: {},
			}),
		),
	);
}

/**
 * Reads the rendered list as a sequence of action aliases, with '|' marking a separator.
 * @param {UmbEntityActionListElement} element - The rendered entity action list.
 * @returns {Array<string>} The sequence.
 */
function readSequence(element: UmbEntityActionListElement): Array<string> {
	const slot = element.shadowRoot!.querySelector('umb-extension-with-api-slot')!;
	return Array.from(slot.shadowRoot!.children).map((child) =>
		child.getAttribute('role') === 'separator'
			? '|'
			: child instanceof UmbTestEntityActionElement
				? (child as any).manifest.alias
				: child.tagName,
	);
}

describe('UmbEntityActionListElement', () => {
	let aliases: Array<string> = [];

	async function renderList(actions: Array<TestAction>) {
		aliases = actions.map((a) => a.alias);
		registerActions(actions);
		const host = await fixture<UmbTestEntityActionListHostElement>(
			html`<umb-test-entity-action-list-host></umb-test-entity-action-list-host>`,
		);
		await sleep(100);
		return host.shadowRoot!.querySelector('umb-entity-action-list') as UmbEntityActionListElement;
	}

	afterEach(() => {
		umbExtensionsRegistry.unregisterMany(aliases);
	});

	it('is defined with its own instance', async () => {
		const element = await renderList([]);
		expect(element).to.be.instanceOf(UmbEntityActionListElement);
	});

	it('renders the test action element', async () => {
		const element = await renderList([{ alias: 'a', weight: 1 }]);
		const slot = element.shadowRoot!.querySelector('umb-extension-with-api-slot')!;
		expect(slot.shadowRoot!.firstElementChild).to.be.instanceOf(UmbTestEntityActionElement);
	});

	it('renders no separators when no action sets separatorBefore', async () => {
		const element = await renderList([
			{ alias: 'a', weight: 3 },
			{ alias: 'b', weight: 2 },
			{ alias: 'c', weight: 1 },
		]);
		expect(readSequence(element)).to.deep.equal(['a', 'b', 'c']);
	});

	it('renders a separator above each action that sets separatorBefore', async () => {
		const element = await renderList([
			{ alias: 'a', weight: 4 },
			{ alias: 'b', weight: 3, separatorBefore: true },
			{ alias: 'c', weight: 2 },
			{ alias: 'd', weight: 1, separatorBefore: true },
		]);
		expect(readSequence(element)).to.deep.equal(['a', '|', 'b', 'c', '|', 'd']);
	});

	it('never renders a separator above the first action', async () => {
		const element = await renderList([
			{ alias: 'a', weight: 2, separatorBefore: true },
			{ alias: 'b', weight: 1 },
		]);
		expect(readSequence(element)).to.deep.equal(['a', 'b']);
	});

	it('renders a separator only while its action is rendered', async () => {
		const element = await renderList([
			{ alias: 'a', weight: 3 },
			{ alias: 'c', weight: 1 },
		]);
		expect(readSequence(element)).to.deep.equal(['a', 'c']);

		aliases.push('b');
		registerActions([{ alias: 'b', weight: 2, separatorBefore: true }]);
		await sleep(100);
		expect(readSequence(element)).to.deep.equal(['a', '|', 'b', 'c']);

		umbExtensionsRegistry.unregister('b');
		await sleep(100);
		expect(readSequence(element)).to.deep.equal(['a', 'c']);
	});
});
