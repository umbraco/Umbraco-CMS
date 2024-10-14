import './document-save-modal.element.js';

import { UmbDocumentVariantState } from '../../types.js';
import type { UmbDocumentSaveModalData, UmbDocumentSaveModalValue } from './document-save-modal.token.js';
import type { UmbDocumentSaveModalElement } from './document-save-modal.element.js';
import type { Meta, StoryObj } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

const modalData: UmbDocumentSaveModalData = {
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
		{
			unique: 'en-gb',
			culture: 'en-gb',
			segment: null,
			variant: {
				name: 'English (GB)',
				culture: 'en-us',
				segment: null,
				state: UmbDocumentVariantState.DRAFT,
				createDate: '2021-08-25T14:00:00Z',
				publishDate: null,
				updateDate: null,
			},
			language: {
				entityType: 'language',
				name: 'English (GB)',
				unique: 'en-gb',
				isDefault: true,
				isMandatory: false,
				fallbackIsoCode: null,
			},
		},
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

const modalValue: UmbDocumentSaveModalValue = {
	selection: ['en-us'],
};

const meta: Meta<UmbDocumentSaveModalElement> = {
	title: 'Workspaces/Document/Modals/Save',
	component: 'umb-document-save-modal',
	id: 'umb-document-save-modal',
	args: {
		data: modalData,
		value: modalValue,
	},
	decorators: [(Story) => html`<div style="border: 1px solid #000;">${Story()}</div>`],
	parameters: {
		layout: 'centered',
		docs: {
			source: {
				language: 'ts',
				code: `
import { UMB_DOCUMENT_PUBLISH_MODAL, UmbDocumentVariantState } from '@umbraco-cms/backoffice/document';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (modalManager) => {
	const result = modalManager.open(this, UMB_DOCUMENT_PUBLISH_MODAL, {
		data: {
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
		}
	}).onSubmit().catch(() => undefined);
});
				`,
			},
		},
	},
};

export default meta;
type Story = StoryObj<UmbDocumentSaveModalElement>;

export const Overview: Story = {};
