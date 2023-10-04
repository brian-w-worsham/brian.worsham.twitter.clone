export class EditProfileModel {
  constructor(
    public UserId: number,
    public UserName: string,
    public Bio: string,
    public ProfilePictureUrl: string,
  ) {}
}
