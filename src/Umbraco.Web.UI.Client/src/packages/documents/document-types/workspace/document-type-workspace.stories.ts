import './document-type-workspace-editor.element.js';
import { Meta, Story } from '@storybook/web-components';
import { treeData } from '../../../../mocks/data/document-type.data.js';
import type { UmbDocumentTypeWorkspaceElement } from './document-type-workspace.element.js';
import { html, ifDefined } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'Workspaces/Document Type',
	component: 'umb-document-type-workspace',
	id: 'umb-document-type-workspace',
} as Meta;

export const AAAOverview: Story<UmbDocumentTypeWorkspaceElement> = () =>
	html` <umb-document-type-workspace id="${ifDefined(treeData[0].id)}"></umb-document-type-workspace>`;
AAAOverview.storyName = 'Overview';
