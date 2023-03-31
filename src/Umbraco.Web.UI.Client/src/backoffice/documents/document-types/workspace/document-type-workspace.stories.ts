import './document-type-workspace-editor.element';
import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';
import { ifDefined } from 'lit/directives/if-defined.js';
import { treeData } from '../../../../core/mocks/data/document-type.data';
import type { UmbDocumentTypeWorkspaceElement } from './document-type-workspace.element';

export default {
	title: 'Workspaces/Document Type',
	component: 'umb-document-type-workspace',
	id: 'umb-document-type-workspace',
} as Meta;

export const AAAOverview: Story<UmbDocumentTypeWorkspaceElement> = () =>
	html` <umb-document-type-workspace id="${ifDefined(treeData[0].key)}"></umb-document-type-workspace>`;
AAAOverview.storyName = 'Overview';
