import type { UmbSectionSidebarElement } from './section-sidebar.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './section-sidebar.element.js';

export default {
	title: 'Sections/Shared/Section Sidebar',
	component: 'umb-section-sidebar',
	id: 'umb-section-sidebar',
} as Meta;

export const AAAOverview: StoryFn<UmbSectionSidebarElement> = () => html`
	<umb-section-sidebar>Section Sidebar Area</umb-section-sidebar>
`;
AAAOverview.storyName = 'Overview';
