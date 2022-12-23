import './workspace-document-type.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { data } from '../../../../../core/mocks/data/document-type.data';

import type { UmbWorkspaceDocumentTypeElement } from './workspace-document-type.element';

export default {
	title: 'Workspaces/Document Type',
	component: 'umb-workspace-document-type',
	id: 'umb-workspace-document-type',
} as Meta;

export const AAAOverview: Story<UmbWorkspaceDocumentTypeElement> = () =>
	html` <umb-workspace-document-type id="${data[0].key}"></umb-workspace-document-type>`;
AAAOverview.storyName = 'Overview';
