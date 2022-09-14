import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbPropertyEditorContextExampleElement } from './property-editor-context-example.element';
import './property-editor-context-example.element';
import { UmbNotificationService } from '../../../core/services/notification';
import '../../../backoffice/components/backoffice-notification-container.element';

export default {
	title: 'Property Editors/Context Example',
	component: 'umb-property-editor-context-example',
	id: 'umb-property-editor-context-example',
	decorators: [
		(story) =>
			html`<umb-context-provider key="umbNotificationService" .value=${new UmbNotificationService()}>
				${story()}
				<umb-backoffice-notification-container></umb-backoffice-notification-container>
			</umb-context-provider>`,
	],
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorContextExampleElement> = () =>
	html` <umb-property-editor-context-example></umb-property-editor-context-example>`;
AAAOverview.storyName = 'Overview';
