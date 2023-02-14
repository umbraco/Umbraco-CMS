import './document-type-workspace.element';
import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';
import type { UmbDocumentTypeWorkspaceElement } from './document-type-workspace.element';
import { treeData } from 'src/core/mocks/data/document-type.data';

export default {
	title: 'Workspaces/Document Type',
	component: 'umb-document-type-workspace',
	id: 'umb-document-type-workspace',
} as Meta;

export const AAAOverview: Story<UmbDocumentTypeWorkspaceElement> = () =>
	html` <umb-document-type-workspace id="${treeData[0].key}"></umb-document-type-workspace>`;
AAAOverview.storyName = 'Overview';
