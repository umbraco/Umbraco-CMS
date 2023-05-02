import { manifests as debugManifests } from './debug/manifests';

// TODO: we need to figure out what components should be available for extensions and load them upfront
// TODO: we need to move these files into their respective folders/silos. We then need a way for a silo to globally register a component
import './body-layout/body-layout.element';
import './footer-layout/footer-layout.element';
import './entity-action/entity-action-list.element';
import './entity-action/entity-action.element';
import './backoffice-frame/backoffice-header.element';
import './backoffice-frame/backoffice-main.element';
import './backoffice-frame/backoffice-modal-container.element';
import './backoffice-frame/backoffice-notification-container.element';
import './button-with-dropdown/button-with-dropdown.element';
import './code-block/code-block.element';
import './collection/collection.element';
import './collection/collection-toolbar.element';
import './collection/collection-selection-actions.element';
import './debug/debug.element';
import './donut-chart';
import './dropdown/dropdown.element';
import './empty-state/empty-state.element';
import './extension-slot/extension-slot.element';
import './input-checkbox-list/input-checkbox-list.element';
import './input-color-picker/input-color-picker.element';
import './input-culture-select/input-culture-select.element';
import './input-document-picker/input-document-picker.element';
import './input-document-type-picker/input-document-type-picker.element';
import './input-eye-dropper/input-eye-dropper.element';
import './input-language-picker/input-language-picker.element';
import './input-media-picker/input-media-picker.element';
import './input-multi-url-picker/input-multi-url-picker.element';
import './input-slider/input-slider.element';
import './input-toggle/input-toggle.element';
import './input-upload-field/input-upload-field.element';
import './input-template/input-template.element';
import './property-type-based-property/property-type-based-property.element';
import './ref-property-editor-ui/ref-property-editor-ui.element';
import './section/section-main/section-main.element';
import './section/section-sidebar/section-sidebar.element';
import './section/section.element';

import './table/table.element';

import './tree/tree.element';
import './tree/entity-tree-item/entity-tree-item.element';
import './tree/tree-menu-item/tree-menu-item.element';

import './entity-bulk-action/entity-bulk-action.element';

import './menu/menu-item-base/menu-item-base.element';

import './variantable-property/variantable-property.element';

import './header-app/header-app-button.element';

import './history/history-list.element';
import './history/history-item.element';

import './workspace/workspace-action/workspace-action.element';
import './workspace/workspace-editor/workspace-editor.element';
import './workspace/workspace-footer/workspace-footer.element';
import './workspace/workspace-action-menu/workspace-action-menu.element';

import './variant-selector/variant-selector.element.ts';

import './template-card/template-card.element';
import './code-editor';

import './tooltip-menu/tooltip-menu.element';

export const manifests = [...debugManifests];
