import './document-workspace.element';
import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';
import { data as documentNodes } from '../../../../core/mocks/data/document.data';
import type { UmbDocumentWorkspaceElement } from './document-workspace.element';

export default {
	title: 'Workspaces/Document',
	component: 'umb-document-workspace',
	id: 'umb-document-workspace',
} as Meta;

export const AAAOverview: Story<UmbDocumentWorkspaceElement> = () =>
	html` <umb-document-workspace id="${documentNodes[0].key}"></umb-document-workspace>`;
AAAOverview.storyName = 'Overview';
