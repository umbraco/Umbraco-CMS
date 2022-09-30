import { expect, fixture, html } from '@open-wc/testing';
import { defaultA11yConfig } from '../../../core/helpers/chai';
import { UmbPropertyEditorUIToggleElement } from './umb-property-editor-ui-toggle.element';

describe('UmbPropertyEditorUIToggleElement', () => {
  let element: UmbPropertyEditorUIToggleElement;

  beforeEach(async () => {
    element = await fixture(
      html` <umb-property-editor-ui-toggle></umb-property-editor-ui-toggle> `
    );
  });

  it('is defined with its own instance', () => {
    expect(element).to.be.instanceOf(UmbPropertyEditorUIToggleElement);
  });

  it('passes the a11y audit', async () => {
    await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
  });
});
