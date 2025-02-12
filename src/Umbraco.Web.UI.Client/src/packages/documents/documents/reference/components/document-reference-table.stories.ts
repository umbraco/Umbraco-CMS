import type { UmbDocumentReferenceTableElement } from './document-reference-table.element.js';
import type { Meta, StoryObj } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './document-reference-table.element.js';

const meta: Meta<UmbDocumentReferenceTableElement> = {
	id: 'umb-document-reference-table',
	title: 'Components/Document/Reference Table',
	component: 'umb-document-reference-table',
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
<umb-document-reference-table unique="<Content GUID>"></umb-document-reference-table>
				`,
			},
		},
	},
};

export default meta;

type Story = StoryObj<UmbDocumentReferenceTableElement>;

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
<umb-document-reference-table unique="<Content GUID>" style="--uui-table-cell-padding:0"></umb-document-reference-table>
				`,
			},
		},
	},
};
