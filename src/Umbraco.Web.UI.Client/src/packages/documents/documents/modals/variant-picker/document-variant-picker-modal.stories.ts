import '../../../../core/components/body-layout/body-layout.element.js';
import './document-variant-picker-modal.element.js';

import type { Meta, StoryObj } from '@storybook/web-components';
import { UmbDocumentVariantState } from '../../types.js';
import type { UmbDocumentVariantPickerModalElement } from './document-variant-picker-modal.element.js';
import type {
	UmbDocumentVariantPickerModalData,
	UmbDocumentVariantPickerModalValue,
} from './document-variant-picker-modal.token.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

const modalData: UmbDocumentVariantPickerModalData = {
	type: 'save',
	options: [
		{
			unique: 'en-us',
			culture: 'en-us',
			segment: null,
			variant: {
				name: 'English variant name',
				culture: 'en-us',
				state: UmbDocumentVariantState.PUBLISHED,
				createDate: '2021-08-25T14:00:00Z',
				publishDate: null,
				updateDate: null,
				segment: null,
			},
			language: {
				entityType: 'language',
				name: 'English',
				unique: 'en-us',
				isDefault: true,
				isMandatory: true,
				fallbackIsoCode: null,
			},
		},
		/*
		// TODO: We do not support segments currently
		{
			name: 'English',
			culture: 'en-us',
			state: UmbDocumentVariantState.DRAFT,
			createDate: '2021-08-25T14:00:00Z',
			publishDate: null,
			updateDate: null,
			segment: 'GTM',
		},
		*/
		{
			unique: 'da-dk',
			culture: 'da-dk',
			segment: null,
			variant: {
				name: 'Danish variant name',
				culture: 'da-dk',
				state: UmbDocumentVariantState.NOT_CREATED,
				createDate: null,
				publishDate: null,
				updateDate: null,
				segment: null,
			},
			language: {
				entityType: 'language',
				name: 'Danish',
				unique: 'da-dk',
				isDefault: false,
				isMandatory: false,
				fallbackIsoCode: null,
			},
		},
	],
};

const modalValue: UmbDocumentVariantPickerModalValue = {
	selection: ['en-us'],
};

const meta: Meta<UmbDocumentVariantPickerModalElement> = {
	title: 'Workspaces/Document/Modals/Variant Picker',
	component: 'umb-document-variant-picker-modal',
	id: 'umb-document-variant-picker-modal',
	args: {
		data: modalData,
		value: modalValue,
	},
	decorators: [(Story) => html`<div style="width: 500px; border: 1px solid #000;">${Story()}</div>`],
	parameters: {
		layout: 'centered',
		docs: {
			source: {
				code: `
import { UMB_DOCUMENT_LANGUAGE_PICKER_MODAL, UmbDocumentVariantState } from '@umbraco-cms/backoffice/document';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (modalManager) => {
	modalManager.open(UMB_DOCUMENT_LANGUAGE_PICKER_MODAL, {
		data: {
			type: 'save',
			variants: [
				{
					name: 'English',
					culture: 'en-us',
					state: UmbDocumentVariantState.PUBLISHED,
					createDate: '2021-08-25T14:00:00Z',
					publishDate: '2021-08-25T14:00:00Z',
					updateDate: null,
					segment: null,
				},
				{
					name: 'English',
					culture: 'en-us',
					state: UmbDocumentVariantState.PUBLISHED,
					createDate: '2021-08-25T14:00:00Z',
					publishDate: '2021-08-25T14:00:00Z',
					updateDate: null,
					segment: 'GTM',
				},
				{
					name: 'Danish',
					culture: 'da-dk',
					state: UmbDocumentVariantState.NOT_CREATED,
					createDate: null,
					publishDate: null,
					updateDate: null,
					segment: null,
				},
			],
		}
	}
});
				`,
			},
		},
	},
};

export default meta;
type Story = StoryObj<UmbDocumentVariantPickerModalElement>;

export const Save: Story = {};
export const Publish: Story = {
	args: {
		data: { ...modalData, type: 'publish' },
	},
};
export const Schedule: Story = {
	args: {
		data: { ...modalData, type: 'schedule' },
	},
};
export const Unpublish: Story = {
	args: {
		data: { ...modalData, type: 'unpublish' },
	},
};
