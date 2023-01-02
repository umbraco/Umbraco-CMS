import './document-workspace.element';
import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';
import type { UmbWorkspaceDocumentElement } from './document-workspace.element';
import { data as documentNodes } from 'src/core/mocks/data/document.data';

export default {
	title: 'Workspaces/Document',
	component: 'umb-workspace-document',
	id: 'umb-workspace-document',
} as Meta;

export const AAAOverview: Story<UmbWorkspaceDocumentElement> = () =>
	html` <umb-workspace-document id="${documentNodes[0].key}"></umb-workspace-document>`;
AAAOverview.storyName = 'Overview';
