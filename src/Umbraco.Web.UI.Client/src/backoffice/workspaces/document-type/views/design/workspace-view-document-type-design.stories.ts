import './workspace-view-document-type-design.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { data } from '../../../../../core/mocks/data/document-type.data';
import { UmbDocumentTypeContext } from '../../document-type.context';

import type { UmbWorkspaceViewDocumentTypeDesignElement } from './workspace-view-document-type-design.element';

export default {
	title: 'Workspaces/Document Type/Views/Design',
	component: 'umb-workspace-view-document-type-design',
	id: 'umb-workspace-view-document-type-design',
	decorators: [
		(story) =>
			html`<umb-context-provider key="umbDocumentTypeContext" .value=${new UmbDocumentTypeContext(data[0])}>
				${story()}
			</umb-context-provider>`,
	],
} as Meta;

export const AAAOverview: Story<UmbWorkspaceViewDocumentTypeDesignElement> = () =>
	html` <umb-workspace-view-document-type-design></umb-workspace-view-document-type-design>`;
AAAOverview.storyName = 'Overview';
