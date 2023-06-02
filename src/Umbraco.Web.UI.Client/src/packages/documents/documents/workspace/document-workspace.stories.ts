import './document-workspace-editor.element.js';
import { Meta, Story } from '@storybook/web-components';
import type { UmbDocumentWorkspaceElement } from './document-workspace.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'Workspaces/Document',
	component: 'umb-document-workspace',
	id: 'umb-document-workspace',
} as Meta;

export const AAAOverview: Story<UmbDocumentWorkspaceElement> = () =>
	html` <umb-document-workspace></umb-document-workspace>`;
AAAOverview.storyName = 'Overview';
