import type { Meta, StoryObj } from '@storybook/web-components';
import type { UmbDocumentTrackedReferenceTableElement } from './document-tracked-reference-table.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './document-tracked-reference-table.element.js';

const meta: Meta<UmbDocumentTrackedReferenceTableElement> = {
	id: 'umb-document-tracked-reference-table',
	title: 'Components/Document/Tracked Reference Table',
	component: 'umb-document-tracked-reference-table',
	args: {
		unique: '1234',
	},
	parameters: {
		actions: {
			disabled: true,
		},
		docs: {
			source: {
				language: 'html',
				code: `
<umb-document-tracked-reference-table unique="<Content GUID>"></umb-document-tracked-reference-table>
				`,
			},
		},
	},
};

export default meta;

type Story = StoryObj<UmbDocumentTrackedReferenceTableElement>;

export const Overview: Story = {};

export const SlimTable: Story = {
	decorators: [
		(story) => {
			return html`<div style="--uui-table-cell-padding: 0;">${story()}</div>`;
		},
	],
	parameters: {
		docs: {
			source: {
				language: 'html',
				code: `
<umb-document-tracked-reference-table unique="<Content GUID>" style="--uui-table-cell-padding:0"></umb-document-tracked-reference-table>
				`,
			},
		},
	},
};
