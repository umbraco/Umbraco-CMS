import {MemberValueBuilder} from './memberValueBuilder';
import {MemberVariantBuilder} from './memberVariantBuilder';
import {ensureIdExists} from '../../helpers/BuilderUtils';

export class MemberBuilder {
  memberValueBuilder: MemberValueBuilder[];
  memberVariantBuilder: MemberVariantBuilder[];
  id: string;
  email: string;
  username: string;
  password: string;
  memberTypeId: string;
  groups: string[];
  isApproved: boolean;

  constructor() {
    this.memberValueBuilder = [];
    this.memberVariantBuilder = [];
    this.groups = [];
  }

  addValue() {
    const builder = new MemberValueBuilder(this);
    this.memberValueBuilder.push(builder);
    return builder;
  }

  addVariant() {
    const builder = new MemberVariantBuilder(this);
    this.memberVariantBuilder.push(builder);
    return builder;
  }

  withId(id: string) {
    this.id = id;
    return this;
  }

  withEmail(email: string) {
    this.email = email;
    return this;
  }

  withUsername(username: string) {
    this.username = username;
    return this;
  }

  withPassword(password: string) {
    this.password = password;
    return this;
  }

  withMemberTypeId(memberTypeId: string) {
    this.memberTypeId = memberTypeId;
    return this;
  }

  addGroup(group: string) {
    this.groups.push(group);
    return this;
  }

  withIsApproved(isApproved: boolean) {
    this.isApproved = isApproved;
    return this;
  }

  build() {
    this.id = ensureIdExists(this.id);

    return {
      values: this.memberValueBuilder.map((builder) => {
        return builder.build();
      }) || [],
      variants: this.memberVariantBuilder.map((builder) => {
        return builder.build();
      }) || [],
      id: this.id,
      email: this.email || '',
      username: this.username || '',
      password: this.password || '',
      memberType: {id : this.memberTypeId},
      groups: this.groups || [],
      isApproved: this.isApproved || false
    };
  }
}
