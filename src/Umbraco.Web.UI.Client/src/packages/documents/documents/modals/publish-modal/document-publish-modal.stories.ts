import '../../../../core/components/body-layout/body-layout.element.js';
import './document-publish-modal.element.js';

import type { Meta, StoryObj } from '@storybook/web-components';
import { UmbDocumentVariantState } from '../../types.js';
import type { UmbDocumentPublishModalData, UmbDocumentPublishModalValue } from './document-publish-modal.token.js';
import type { UmbDocumentPublishModalElement } from './document-publish-modal.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

const modalData: UmbDocumentPublishModalData = {
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
			unique: 'en-us-gtm',
			culture: 'en-us',
			segment: 'GTM',
			variant: {
				name: 'English',
				culture: 'en-us',
				segment: 'GTM',
				state: UmbDocumentVariantState.DRAFT,
				createDate: '2021-08-25T14:00:00Z',
				publishDate: null,
				updateDate: null,
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
};

const modalValue: UmbDocumentPublishModalValue = {
	selection: ['en-us'],
};

const meta: Meta<UmbDocumentPublishModalElement> = {
	title: 'Workspaces/Document/Modals/Publish',
	component: 'umb-document-publish-modal',
	id: 'umb-document-publish-modal',
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
import { UMB_DOCUMENT_PUBLISH_MODAL, UmbDocumentVariantState } from '@umbraco-cms/backoffice/document';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (modalManager) => {
	modalManager.open(this, UMB_DOCUMENT_PUBLISH_MODAL, {
		data: {
			options: [
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
type Story = StoryObj<UmbDocumentPublishModalElement>;

export const Overview: Story = {};
