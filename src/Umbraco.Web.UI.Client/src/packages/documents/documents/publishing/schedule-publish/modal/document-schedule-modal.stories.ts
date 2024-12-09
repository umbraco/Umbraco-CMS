import './document-schedule-modal.element.js';

import { UmbDocumentVariantState } from '../../../types.js';
import type { UmbDocumentScheduleModalData, UmbDocumentScheduleModalValue } from './document-schedule-modal.token.js';
import type { UmbDocumentScheduleModalElement } from './document-schedule-modal.element.js';
import type { Meta, StoryObj } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

const modalData: UmbDocumentScheduleModalData = {
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
				state: UmbDocumentVariantState.PUBLISHED_PENDING_CHANGES,
				createDate: '2021-08-25T14:00:00Z',
				publishDate: null,
				updateDate: null,
				segment: null,
			},
			language: {
				entityType: 'language',
				name: 'Danish',
				unique: 'da-dk',
				isDefault: true,
				isMandatory: false,
				fallbackIsoCode: null,
			},
		},
	],
};

const modalValue: UmbDocumentScheduleModalValue = {
	selection: [{ unique: 'en-us', schedule: null }],
};

const meta: Meta<UmbDocumentScheduleModalElement> = {
	title: 'Workspaces/Document/Modals/Schedule',
	component: 'umb-document-schedule-modal',
	id: 'umb-document-schedule-modal',
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
import { UMB_DOCUMENT_SCHEDULE_MODAL, UmbDocumentVariantState } from '@umbraco-cms/backoffice/document';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (modalManager) => {
	const result = await modalManager.open(this, UMB_DOCUMENT_SCHEDULE_MODAL, {
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
			],
		},
		value: {
			selection: [{ unique: 'en-us', schedule: null }],
		},
	}).onSubmit().catch(() => undefined);
});
				`,
			},
		},
	},
};

export default meta;
type Story = StoryObj<UmbDocumentScheduleModalElement>;

export const Overview: Story = {};

export const WithoutVariants: Story = {
	args: {
		data: {
			options: modalData.options.slice(0, 1),
		},
	},
};
