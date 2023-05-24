import './document-workspace-editor.element';
import { Meta, Story } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';
import type { UmbDocumentWorkspaceElement } from './document-workspace.element.js';

export default {
	title: 'Workspaces/Document',
	component: 'umb-document-workspace',
	id: 'umb-document-workspace',
} as Meta;

export const AAAOverview: Story<UmbDocumentWorkspaceElement> = () =>
	html` <umb-document-workspace></umb-document-workspace>`;
AAAOverview.storyName = 'Overview';
