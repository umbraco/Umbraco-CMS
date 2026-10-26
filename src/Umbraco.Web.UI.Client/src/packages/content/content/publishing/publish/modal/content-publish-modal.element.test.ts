import { UmbContentPublishModalElement } from './content-publish-modal.element.js';
import type { UmbContentPublishModalData, UmbContentPublishModalValue } from './types.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { umbLocalizationManager, type UmbLocalizationSet } from '@umbraco-cms/backoffice/localization-api';
import { UmbPublishableVariantState, type UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';

function createOption(culture: string | null): UmbEntityVariantOptionModel {
	return {
		culture,
		segment: null,
		unique: culture ?? 'invariant',
		language: { unique: culture ?? 'invariant', name: culture ?? 'Invariant', isMandatory: false },
		variant: { culture, segment: null, name: 'Name', state: UmbPublishableVariantState.DRAFT },
	} as unknown as UmbEntityVariantOptionModel;
}

/** Stands in for the modal context, which holds the modal's value. */
function createModalContext(value: UmbContentPublishModalValue) {
	const state = new UmbObjectState(value);
	return {
		value: state.asObservable(),
		setValue: (newValue: UmbContentPublishModalValue) => state.setValue(newValue),
		updateValue: (partialValue: Partial<UmbContentPublishModalValue>) => state.update(partialValue),
		submit: () => {},
		reject: () => {},
	};
}

async function createModal(data: UmbContentPublishModalData, value: UmbContentPublishModalValue) {
	const element = await fixture<UmbContentPublishModalElement>(
		html`<umb-content-publish-modal
			.data=${data}
			.modalContext=${createModalContext(value) as never}></umb-content-publish-modal>`,
	);
	// The selection is applied in firstUpdated, which schedules another render.
	await aTimeout(0);
	await element.updateComplete;
	return element;
}

function getConfirmButton(element: UmbContentPublishModalElement) {
	return element.shadowRoot!.querySelector('uui-button[look="primary"]') as HTMLElement & { disabled: boolean };
}

function getDescription(element: UmbContentPublishModalElement) {
	return element.shadowRoot!.querySelector('uui-dialog-layout > p')!.textContent?.trim();
}

function getSelectAllCheckbox(element: UmbContentPublishModalElement) {
	const picker = element.shadowRoot!.querySelector('umb-content-variant-language-picker')!;
	return picker.shadowRoot!.querySelector('uui-checkbox') as HTMLElement & { disabled: boolean };
}

describe('UmbContentPublishModalElement', () => {
	before(() => {
		const localization: UmbLocalizationSet = {
			$code: 'en',
			$dir: 'ltr',
			prompt_confirmPublish: 'Default confirmation',
			testCustomConfirmation: 'Custom confirmation',
		};
		umbLocalizationManager.registerLocalization(localization);
	});

	it('is defined with its own instance', async () => {
		const element = await createModal({ options: [createOption(null)] }, { selection: [] });
		expect(element).to.be.instanceOf(UmbContentPublishModalElement);
	});

	it('disables the confirm button when no variant can be selected', async () => {
		const element = await createModal(
			{ options: [createOption('en-US'), createOption('da-DK')], pickableFilter: () => false },
			{ selection: ['en-US'] },
		);

		expect(getConfirmButton(element).disabled).to.be.true;
	});

	it('disables Select all when no variant can be selected', async () => {
		const element = await createModal(
			{ options: [createOption('en-US'), createOption('da-DK')], pickableFilter: () => false },
			{ selection: [] },
		);

		expect(getSelectAllCheckbox(element).disabled).to.be.true;
	});

	it('enables Select all when a variant can be selected', async () => {
		const element = await createModal(
			{ options: [createOption('en-US'), createOption('da-DK')], pickableFilter: (option) => option.culture === 'da-DK' },
			{ selection: [] },
		);

		expect(getSelectAllCheckbox(element).disabled).to.be.false;
	});

	it('enables the confirm button when a variant is selected', async () => {
		const element = await createModal(
			{ options: [createOption('en-US'), createOption('da-DK')] },
			{ selection: ['en-US'] },
		);

		expect(getConfirmButton(element).disabled).to.be.false;
	});

	it('enables the confirm button for invariant content', async () => {
		const element = await createModal({ options: [createOption(null)] }, { selection: [] });

		expect(getConfirmButton(element).disabled).to.be.false;
	});

	it('shows the default confirmation when no description is given', async () => {
		const element = await createModal({ options: [createOption(null)] }, { selection: [] });

		expect(getDescription(element)).to.equal('Default confirmation');
	});

	it('shows the given description instead of the default confirmation', async () => {
		const element = await createModal(
			{ options: [createOption(null)], description: '#testCustomConfirmation' },
			{ selection: [] },
		);

		expect(getDescription(element)).to.equal('Custom confirmation');
	});
});
