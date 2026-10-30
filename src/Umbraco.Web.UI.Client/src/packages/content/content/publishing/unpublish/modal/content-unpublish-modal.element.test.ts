import type { UmbContentUnpublishModalElement } from './content-unpublish-modal.element.js';
import type { UmbContentUnpublishModalData, UmbContentUnpublishModalValue } from './types.js';
import './content-unpublish-modal.element.js';
import { aTimeout, expect } from '@open-wc/testing';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbPublishableVariantState } from '@umbraco-cms/backoffice/variant';
import type { UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';

const option = (culture: string, isMandatory = false): UmbEntityVariantOptionModel =>
	({
		unique: culture,
		culture,
		segment: null,
		language: {
			unique: culture,
			name: culture,
			entityType: 'language',
			isDefault: false,
			isMandatory,
			fallbackIsoCode: null,
		},
		variant: { culture, segment: null, name: culture, state: UmbPublishableVariantState.PUBLISHED },
	}) as unknown as UmbEntityVariantOptionModel;

describe('UmbContentUnpublishModalElement', () => {
	let element: UmbContentUnpublishModalElement;
	let submitted: UmbContentUnpublishModalValue | undefined;

	const render = async (selection: Array<string>, data: Partial<UmbContentUnpublishModalData> = {}) => {
		submitted = undefined;
		element = document.createElement('umb-content-unpublish-modal');
		element.modalContext = {
			value: new UmbObjectState<UmbContentUnpublishModalValue>({ selection }).asObservable(),
			setValue: (value: UmbContentUnpublishModalValue) => (submitted = value),
			submit: () => {},
			reject: () => {},
		} as any;
		element.data = { options: [option('en-US', true), option('da-DK'), option('de-DE')], ...data };
		document.body.appendChild(element);
		await element.updateComplete;
		await aTimeout(0);
	};

	const item = (label: string) => {
		const picker = element.shadowRoot!.querySelector('umb-content-variant-language-picker')!;
		const menuItem = picker.shadowRoot!.querySelector<HTMLElement>(`uui-menu-item[label="${label}"]`);
		expect(menuItem, label).to.exist;
		return menuItem!;
	};

	const toggle = async (label: string, event: 'selected' | 'deselected') => {
		item(label).dispatchEvent(new Event(event));
		await aTimeout(0);
	};

	const submit = async () => {
		element.shadowRoot!.querySelector<HTMLElement>('uui-button[look="primary"]')!.click();
		await aTimeout(0);
		return [...(submitted?.selection ?? [])].sort();
	};

	afterEach(() => element.remove());

	it('selects every other language when the initial selection includes a mandatory language', async () => {
		await render(['en-US']);

		expect(await submit()).to.deep.equal(['da-DK', 'de-DE', 'en-US']);
	});

	it('selects every other language when a mandatory language is selected', async () => {
		await render(['da-DK']);
		await toggle('en-US', 'selected');

		expect(item('de-DE').hasAttribute('selected'), 'shown as selected').to.be.true;
		expect(await submit()).to.deep.equal(['da-DK', 'de-DE', 'en-US']);
	});

	it('keeps every other language selected while a mandatory language is selected', async () => {
		await render(['en-US']);
		item('da-DK').shadowRoot!.querySelector<HTMLElement>('#label-button')!.click();
		await aTimeout(0);

		expect(item('da-DK').hasAttribute('selected'), 'shown as selected').to.be.true;
		expect(await submit()).to.deep.equal(['da-DK', 'de-DE', 'en-US']);
	});

	it('deselects only the languages it selected when the mandatory language is deselected', async () => {
		await render(['da-DK']);
		await toggle('en-US', 'selected');
		await toggle('en-US', 'deselected');

		expect(await submit()).to.deep.equal(['da-DK']);
	});

	it('leaves the selection alone when no mandatory language is selected', async () => {
		await render(['da-DK']);

		expect(await submit()).to.deep.equal(['da-DK']);
	});

	it('does not select languages the user cannot pick', async () => {
		await render(['en-US'], { pickableFilter: (o) => o.culture !== 'de-DE' });

		expect(await submit()).to.deep.equal(['da-DK', 'en-US']);
	});

	it('emits the selected languages consistently to every observer of the selection', async () => {
		await render(['da-DK']);
		let observed: Array<string> = [];
		const subscription = (element as any)._selectionManager.selection.subscribe(
			(selection: Array<string>) => (observed = selection),
		);
		await toggle('en-US', 'selected');
		subscription.unsubscribe();

		expect([...observed].sort()).to.deep.equal(['da-DK', 'de-DE', 'en-US']);
	});
});
